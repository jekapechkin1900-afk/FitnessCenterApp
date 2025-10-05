using System.Net.Sockets;
using System.Text;
using FitnessCenterApp.Shared.Helpers;
using FitnessCenterApp.Shared.RPC;

namespace FitnessCenterApp.Shared.Clients;

public class RpcClient(string host, int port)
{
	private readonly string _host = host;
	private readonly int _port = port;

	public async Task<RpcResponse?> SendRequestAsync(RpcRequest request)
	{
		using var tcpClient = new TcpClient();
		await tcpClient.ConnectAsync(_host, _port);
		using var stream = tcpClient.GetStream();

		var requestJson = JsonHelper.Serialize(request);
		var requestBytes = Encoding.UTF8.GetBytes(requestJson);
		var lengthBytes = BitConverter.GetBytes(requestBytes.Length);

		await stream.WriteAsync(lengthBytes.AsMemory(0, 4));
		await stream.WriteAsync(requestBytes);

		var responseLengthBuffer = new byte[4];
		await stream.ReadExactlyAsync(responseLengthBuffer, 0, 4);
		var responseLength = BitConverter.ToInt32(responseLengthBuffer, 0);

		var responseBuffer = new byte[responseLength];
		await stream.ReadExactlyAsync(responseBuffer, 0, responseLength);
		var responseJson = Encoding.UTF8.GetString(responseBuffer);

		return JsonHelper.Deserialize<RpcResponse>(responseJson);
	}
}
