using System.Net;
using System.Net.Sockets;
using FitnessCenterApp.AnalyticsService.Application.Interfaces.Services;

namespace FitnessCenterApp.AnalyticsService;

public class RpcServer(string ipAddress, int port, IAnalyticsService analyticsService)
{
	private readonly TcpListener _listener = new TcpListener(IPAddress.Parse(ipAddress), port);
	private readonly IAnalyticsService _analyticsService = analyticsService;

	public async Task StartAsync()
	{
		_listener.Start();
		Console.WriteLine($"AnalyticsService RPC Server started on {_listener.LocalEndpoint}");
		while (true)
		{
			var tcpClient = await _listener.AcceptTcpClientAsync();
			Console.WriteLine("Client connected...");
			var handler = new RequestHandler(tcpClient, _analyticsService);
			_ = handler.HandleAsync();
		}
	}
}