using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using FitnessCenterApp.Shared.Http;

namespace FitnessCenterApp.Shared.Client;

public abstract class ApiServiceBase
{
	private readonly int _port;
	private readonly string _serviceName;
	private readonly string _swaggerHtml;

	protected readonly JsonSerializerOptions _jsonOptions = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
		WriteIndented = true
	};

	protected ApiServiceBase(int port, string serviceName)
	{
		_port = port;
		_serviceName = serviceName;
		_swaggerHtml = LoadAndPrepareSwaggerTemplate();
	}

	public void Start()
	{
		var listener = new TcpListener(IPAddress.Any, _port);
		listener.Start();
		Console.WriteLine($"{_serviceName} запущен на порту {_port}.");
		Console.WriteLine($"Интерфейс API доступен по адресу: http://localhost:{_port}/");

		while (true)
		{
			var client = listener.AcceptTcpClient();
			_ = Task.Run(() => HandleClient(client));
		}
	}

	private async Task HandleClient(TcpClient client)
	{
		await using var stream = client.GetStream();
		try
		{
			var request = await HttpParser.ParseRequest(stream);
			if (request != null)
			{
				await RouteRequest(stream, request);
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"[{_serviceName}] Ошибка обработки клиента: {ex.Message}");
			if (stream.CanWrite)
			{
				await WriteJsonResponse(stream, "500 Internal Server Error", new { error = "An internal server error occurred." });
			}
		}
		finally
		{
			client.Close();
		}
	}

	private async Task RouteRequest(Stream stream, HttpRequest request)
	{
		Console.WriteLine($"[{_serviceName}] Входящий запрос: {request.Method} {request.Path}");

		if (request.Method == "OPTIONS")
		{
			await HandleCorsPreflight(stream);
			return;
		}

		if (request.Path == "/")
		{
			await WriteHttpResponse(stream, "200 OK", "text/html; charset=utf-8", Encoding.UTF8.GetBytes(_swaggerHtml));
		}
		else
		{
			await RouteApiRequest(stream, request);
		}
	}

	private async Task HandleCorsPreflight(Stream stream)
	{
		
		var response = "HTTP/1.1 204 No Content\r\n" +
					   "Access-Control-Allow-Origin: *\r\n" + 
					   "Access-Control-Allow-Methods: GET, POST, PUT, DELETE, OPTIONS\r\n" + 
					   "Access-Control-Allow-Headers: Content-Type\r\n" +
					   "\r\n";
		var headerBytes = Encoding.UTF8.GetBytes(response);
		await stream.WriteAsync(headerBytes);
		await stream.FlushAsync();
	}

	protected abstract Task RouteApiRequest(Stream stream, HttpRequest request);

	protected abstract ApiEndpoint[] GetApiEndpoints();

	#region HTTP Response Helpers

	protected async Task WriteJsonResponse(Stream stream, string statusCode, object data)
	{
		var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(data, _jsonOptions);
		await WriteHttpResponse(stream, statusCode, "application/json; charset=utf-8", jsonBytes);
	}

	protected async Task WriteHttpResponse(Stream stream, string statusCode, string contentType, byte[] body)
	{
		try
		{
			var response = $"HTTP/1.1 {statusCode}\r\n" +
					   $"Content-Type: {contentType}\r\n" +
					   $"Content-Length: {body.Length}\r\n" +
					   "Connection: close\r\n" +
					   "Access-Control-Allow-Origin: *\r\n" +
					   "\r\n";

			var headerBytes = Encoding.UTF8.GetBytes(response);
			await stream.WriteAsync(headerBytes, 0, headerBytes.Length);
			if (body.Length > 0)
			{
				await stream.WriteAsync(body, 0, body.Length);
			}
			await stream.FlushAsync();
		}
		catch (IOException ex)
		{
			Console.WriteLine($"[INFO] Не удалось записать ответ: {ex.Message}");
		}
	}

	#endregion

	#region Swagger Page Generation

	private string LoadAndPrepareSwaggerTemplate()
	{
		var assembly = GetType().Assembly; 
		var resourceName = assembly.GetManifestResourceNames().FirstOrDefault(s => s.EndsWith("SwaggerTemplate.html"));

		if (resourceName == null)
		{
			assembly = Assembly.GetExecutingAssembly();
			resourceName = "FitnessCenterApp.Shared.SwaggerTemplate.html";
		}

		using var stream = assembly.GetManifestResourceStream(resourceName);
		if (stream == null) throw new FileNotFoundException($"Не удалось найти встроенный ресурс SwaggerTemplate.html в сборке {assembly.FullName}. Убедитесь, что для файла установлено 'Действие при сборке' = 'Внедренный ресурс'.");

		using var reader = new StreamReader(stream);
		var template = reader.ReadToEnd();

		template = template.Replace("{{ServiceName}}", _serviceName);
		template = template.Replace("{{ServicePort}}", _port.ToString());

		var endpointsHtml = new StringBuilder();
		foreach (var endpoint in GetApiEndpoints())
		{
			endpointsHtml.Append(GenerateEndpointHtml(endpoint));
		}
		template = template.Replace("{{Endpoints}}", endpointsHtml.ToString());

		return template;
	}

	private string GenerateEndpointHtml(ApiEndpoint endpoint)
	{
		var methodClass = endpoint.Method.ToLower();
		var bodyHtml = !string.IsNullOrEmpty(endpoint.ExampleBody)
			? $"<textarea class='body-textarea' placeholder='Тело запроса в формате JSON...'>{endpoint.ExampleBody}</textarea>"
			: "";

		var pathParamHtml = endpoint.Path.Contains("{id}")
			? $"<input type='text' class='path-param' placeholder='Значение для {{id}}' style='margin-right: 10px;'>"
			: "";

		return $@"
			<div class='endpoint'>
				<div class='endpoint-header'>
					<span class='method {methodClass}'>{endpoint.Method}</span>
					<span class='path'>{endpoint.Path}</span>
					<span>{endpoint.Description}</span>
				</div>
				<div class='endpoint-body'>
					{bodyHtml}
					{pathParamHtml}
					<button class='try-it-out' data-method='{endpoint.Method}'>Выполнить</button>
					<h4>Результат:</h4>
					<pre class='result-pre'>Ожидание запроса...</pre>
				</div>
			</div>";
	}

	#endregion

}


