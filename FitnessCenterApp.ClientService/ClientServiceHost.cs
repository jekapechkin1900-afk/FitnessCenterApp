using System.Net;
using System.Net.Sockets;
using FitnessCenterApp.ClientService.Application.Interfaces.Services;
namespace FitnessCenterApp.ClientService;

public class RpcServer(string ipAddress, int port, IClientService clientService, IMembershipService membershipService)
{
	private readonly TcpListener _listener = new(IPAddress.Parse(ipAddress), port);
	private readonly IClientService _clientService = clientService;
	private readonly IMembershipService _membershipService = membershipService;

	public async Task StartAsync()
	{
		_listener.Start();
		Console.WriteLine($"ClientService RPC Server started on {_listener.LocalEndpoint}");
		while (true)
		{
			var tcpClient = await _listener.AcceptTcpClientAsync();
			Console.WriteLine("Client connected...");
			var handler = new RequestHandler(tcpClient, _clientService, _membershipService);
			_ = handler.HandleAsync();
		}
	}
}
