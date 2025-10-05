using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Web;
using FitnessCenterApp.Shared.Clients;
using FitnessCenterApp.Shared.Helpers;
using FitnessCenterApp.Shared.Http;
using FitnessCenterApp.Shared.RPC;

namespace FitnessCenterApp.ProxyServer;

public class HttpGatewayServer
{
	private readonly TcpListener _listener;
	private readonly Dictionary<string, (string host, int port)> _serviceRegistry = [];

	public HttpGatewayServer(string ipAddress, int port)
	{
		_listener = new TcpListener(IPAddress.Parse(ipAddress), port);

		_serviceRegistry["clients"] = ("127.0.0.1", 9001);
		_serviceRegistry["memberships"] = ("127.0.0.1", 9001);
		_serviceRegistry["analytics"] = ("127.0.0.1", 9002);
		_serviceRegistry["visits"] = ("127.0.0.1", 9002);
	}

	public async Task StartAsync()
	{
		_listener.Start();
		Console.WriteLine($"Gateway listening on {_listener.LocalEndpoint}");

		while (true)
		{
			var tcpClient = await _listener.AcceptTcpClientAsync();
			_ = HandleClientAsync(tcpClient);
		}
	}

	private async Task HandleClientAsync(TcpClient tcpClient)
	{
		await using var stream = tcpClient.GetStream();
		try
		{
			var parsedRequest = await HttpRequestParser.ParseAsync(stream);
			Console.WriteLine($"Incoming request: {parsedRequest.Method} {parsedRequest.Path}");

			if (parsedRequest.Method == "OPTIONS")
			{
				await WriteHttpResponseAsync(stream, 204, "No Content", [], "");
				return;
			}

			var rpcRequest = CreateRpcRequestFromHttp(parsedRequest);
			if (rpcRequest == null)
			{
				await WriteErrorResponseAsync(stream, 400, "Bad Request", "Cannot map HTTP request to RPC method.");
				return;
			}

			var (host, port) = _serviceRegistry[rpcRequest.ServiceName];
			var rpcClient = new RpcClient(host, port);
			var rpcResponse = await rpcClient.SendRequestAsync(rpcRequest);

			var httpStatusCode = MapRpcStatusToHttpStatus(rpcResponse.StatusCode);
			await WriteHttpResponseAsync(stream, (int)httpStatusCode, httpStatusCode.ToString(),
				new Dictionary<string, string> { { "Content-Type", "application/json" } }, rpcResponse.Payload);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error processing raw HTTP request: {ex.Message}");
			await WriteErrorResponseAsync(stream, 500, "Internal Server Error", ex.Message);
		}
		finally
		{
			tcpClient.Close();
		}
	}

	private async Task WriteHttpResponseAsync(NetworkStream stream, int statusCode, string statusMessage, Dictionary<string, string> headers, string body)
	{
		var bodyBytes = Encoding.UTF8.GetBytes(body);
		var responseBuilder = new StringBuilder();

		responseBuilder.Append($"HTTP/1.1 {statusCode} {statusMessage}\r\n");
		responseBuilder.Append("Server: FitnessCenterApp Raw TCP Server\r\n");
		responseBuilder.Append("Connection: Close\r\n"); 
		responseBuilder.Append($"Content-Length: {bodyBytes.Length}\r\n");
		responseBuilder.Append("Access-Control-Allow-Origin: *\r\n");
		responseBuilder.Append("Access-Control-Allow-Methods: GET, POST, PUT, DELETE, OPTIONS\r\n");
		responseBuilder.Append("Access-Control-Allow-Headers: Content-Type\r\n");

		foreach (var header in headers)
		{
			responseBuilder.Append($"{header.Key}: {header.Value}\r\n");
		}

		responseBuilder.Append("\r\n"); 

		var headerBytes = Encoding.UTF8.GetBytes(responseBuilder.ToString());

		await stream.WriteAsync(headerBytes);
		if (bodyBytes.Length > 0)
		{
			await stream.WriteAsync(bodyBytes);
		}
		await stream.FlushAsync();
	}

	private Task WriteErrorResponseAsync(NetworkStream stream, int code, string status, string message)
	{
		var errorBody = JsonHelper.Serialize(new { error = message });
		return WriteHttpResponseAsync(stream, code, status,
			new Dictionary<string, string> { { "Content-Type", "application/json" } }, errorBody);
	}

	private RpcRequest CreateRpcRequestFromHttp(ParsedHttpRequest request)
	{
		var pathSegments = request.Path.Trim('/').Split('/');
		if (pathSegments.Length < 2 || pathSegments[0] != "api") return null;

		var serviceKey = pathSegments[1];
		string methodName;
		var payload = request.Body;

		if (serviceKey == "analytics" && request.Method == "GET" && pathSegments.Length == 3 && pathSegments[2] == "visits-in-period")
		{
			methodName = "GetTotalVisitsInPeriod";
			var queryParams = HttpUtility.ParseQueryString(request.Query);
			var startDate = DateTime.Parse(queryParams["start"]);
			var endDate = DateTime.Parse(queryParams["end"]);
			payload = JsonHelper.Serialize(new { Start = startDate, End = endDate });
		}
		else if (serviceKey == "analytics")
		{
			methodName = pathSegments.Length > 2 ? GetAnalyticsMethodName(pathSegments[2]) : null;
		}
		else 
		{
			string entityName = Capitalize(serviceKey).TrimEnd('s');
			methodName = request.Method switch
			{
				"GET" when pathSegments.Length == 2 => $"GetAll{Capitalize(serviceKey)}",
				"GET" when pathSegments.Length == 3 => $"Get{entityName}ById",
				"POST" when pathSegments.Length == 2 => serviceKey == "memberships" ? "AddMembership" : $"Create{entityName}",
				"PUT" when pathSegments.Length == 3 => $"Update{entityName}",
				"DELETE" when pathSegments.Length == 3 => $"Delete{entityName}",
				_ => null
			};

			if (request.Method is "GET" or "DELETE" && pathSegments.Length == 3)
			{
				payload = JsonHelper.Serialize(Guid.Parse(pathSegments[2]));
			}
		}

		if (methodName == null || !_serviceRegistry.ContainsKey(serviceKey)) return null;

		return new RpcRequest { ServiceName = serviceKey, MethodName = methodName, Payload = payload };
	}

	private string? GetAnalyticsMethodName(string route) => route switch
	{
		"logvisit" => "LogVisit",
		"most-active-client" => "GetMostActiveClientId",
		"busiest-day" => "GetBusiestDayOfWeek",
		"most-profitable-month" => "GetMostProfitableMonth",
		"most-popular-membership" => "GetMostPopularMembershipType",
		"visits-in-period" => "GetTotalVisitsInPeriod",
		_ => null
	};

	private string Capitalize(string s) => char.ToUpper(s[0]) + s.Substring(1);

	private HttpStatusCode MapRpcStatusToHttpStatus(RpcStatusCode rpcStatus) => rpcStatus switch
	{
		RpcStatusCode.Ok => HttpStatusCode.OK,
		RpcStatusCode.NotFound => HttpStatusCode.NotFound,
		RpcStatusCode.BadRequest => HttpStatusCode.BadRequest,
		_ => HttpStatusCode.InternalServerError
	};

	private async Task WriteResponseAsync(HttpListenerResponse response, HttpStatusCode statusCode, string content)
	{
		response.StatusCode = (int)statusCode;
		var buffer = Encoding.UTF8.GetBytes(content);
		response.ContentLength64 = buffer.Length;
		await response.OutputStream.WriteAsync(buffer);
	}
}
