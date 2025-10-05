using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using FitnessCenterApp.ClientService.Application.Interfaces.Services;
using FitnessCenterApp.Shared.Contracts;
using FitnessCenterApp.Shared.Helpers;
using FitnessCenterApp.Shared.RPC;

namespace FitnessCenterApp.ClientService;

public class RequestHandler(TcpClient tcpClient, IClientService clientService, IMembershipService membershipService)
{
	private readonly TcpClient _tcpClient = tcpClient;
	private readonly IClientService _clientService = clientService;
	private readonly IMembershipService _membershipService = membershipService;

	public async Task HandleAsync()
	{
		try
		{
			using var stream = _tcpClient.GetStream();
			var lengthBuffer = new byte[4];
			await stream.ReadExactlyAsync(lengthBuffer, 0, 4);
			var messageLength = BitConverter.ToInt32(lengthBuffer, 0);

			var messageBuffer = new byte[messageLength];
			await stream.ReadExactlyAsync(messageBuffer, 0, messageLength);
			var requestJson = Encoding.UTF8.GetString(messageBuffer);
			var request = JsonHelper.Deserialize<RpcRequest>(requestJson);
			ArgumentNullException.ThrowIfNull(request);

			var response = await ProcessRequestAsync(request);

			var responseJson = JsonHelper.Serialize(response);
			var responseBytes = Encoding.UTF8.GetBytes(responseJson);
			var responseLengthBytes = BitConverter.GetBytes(responseBytes.Length);

			await stream.WriteAsync(responseLengthBytes.AsMemory(0, 4));
			await stream.WriteAsync(responseBytes);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error handling request: {ex.Message}");
		}
		finally
		{
			_tcpClient.Close();
		}
	}

	private async Task<RpcResponse> ProcessRequestAsync(RpcRequest request)
	{
		try
		{
			object result = request.MethodName switch
			{
				// Client Service Methods
				"CreateClient" => await _clientService.CreateClientAsync(JsonHelper.Deserialize<ClientDto>(request.Payload)),
				"GetClientById" => await _clientService.GetClientByIdAsync(JsonHelper.Deserialize<Guid>(request.Payload)),
				"GetAllClients" => await _clientService.GetAllClientsAsync(),
				"UpdateClient" => await _clientService.UpdateClientAsync(JsonHelper.Deserialize<ClientDto>(request.Payload)),
				"DeleteClient" => await _clientService.DeleteClientAsync(JsonHelper.Deserialize<Guid>(request.Payload)),

				// Membership Service Methods
				"AddMembership" => await _membershipService.AddMembershipAsync(JsonHelper.Deserialize<MembershipDto>(request.Payload)),
				"GetMembershipById" => await _membershipService.GetMembershipByIdAsync(JsonHelper.Deserialize<Guid>(request.Payload)),
				"GetAllMemberships" => await _membershipService.GetAllMembershipsAsync(),
				"GetMembershipsByClientId" => await _membershipService.GetMembershipsByClientIdAsync(JsonHelper.Deserialize<Guid>(request.Payload)),
				"UpdateMembership" => await _membershipService.UpdateMembershipAsync(JsonHelper.Deserialize<MembershipDto>(request.Payload)),
				"DeleteMembership" => await _membershipService.DeleteMembershipAsync(JsonHelper.Deserialize<Guid>(request.Payload)),

				_ => throw new NotSupportedException($"Method '{request.MethodName}' is not supported.")
			};

			return new RpcResponse
			{
				StatusCode = result != null ? RpcStatusCode.Ok : RpcStatusCode.NotFound,
				Payload = JsonHelper.Serialize(result)
			};
		}
		catch (Exception ex)
		{
			var statusCode = ex switch
			{
				KeyNotFoundException _ => RpcStatusCode.NotFound,
				ArgumentException _ => RpcStatusCode.BadRequest,
				_ => RpcStatusCode.InternalServerError
			};
			return new RpcResponse
			{
				StatusCode = statusCode,
				Payload = JsonHelper.Serialize(ex.Message)
			};
		}
	}
}
