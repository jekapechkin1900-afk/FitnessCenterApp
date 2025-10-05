using System.Net.Sockets;
using System.Text;
using FitnessCenterApp.AnalyticsService.Application.Interfaces.Services;
using FitnessCenterApp.Shared.Contracts;
using FitnessCenterApp.Shared.Helpers;
using FitnessCenterApp.Shared.RPC;

namespace FitnessCenterApp.AnalyticsService;

public class RequestHandler(TcpClient tcpClient, IAnalyticsService analyticsService)
{
	private readonly TcpClient _tcpClient = tcpClient;
	private readonly IAnalyticsService _analyticsService = analyticsService;

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

	private class PeriodRequestDto { public DateTime Start { get; set; } public DateTime End { get; set; } }

	private async Task<RpcResponse> ProcessRequestAsync(RpcRequest request)
	{
		try
		{
			object result = request.MethodName switch
			{
				"CreateVisit" => await _analyticsService.CreateVisitAsync(JsonHelper.Deserialize<VisitDto>(request.Payload)),
				"GetAllVisits" => await _analyticsService.GetAllVisitsAsync(),
				"UpdateVisit" => await _analyticsService.UpdateVisitAsync(JsonHelper.Deserialize<VisitDto>(request.Payload)),
				"DeleteVisit" => await _analyticsService.DeleteVisitAsync(JsonHelper.Deserialize<Guid>(request.Payload)),
				"GetMostActiveClientId" => await _analyticsService.GetMostActiveClientIdAsync(),
				"GetBusiestDayOfWeek" => await _analyticsService.GetBusiestDayOfWeekAsync(),
				"GetMostProfitableMonth" => await _analyticsService.GetMostProfitableMonthAsync(),
				"GetMostPopularMembershipType" => await _analyticsService.GetMostPopularMembershipTypeAsync(),
				"GetTotalVisitsInPeriod" => await HandleGetTotalVisitsInPeriod(request.Payload),
				_ => throw new NotSupportedException($"Method '{request.MethodName}' is not supported.")
			};

			return new RpcResponse
			{
				StatusCode = RpcStatusCode.Ok,
				Payload = JsonHelper.Serialize(result)
			};
		}
		catch (Exception ex)
		{
			return new RpcResponse
			{
				StatusCode = RpcStatusCode.InternalServerError,
				Payload = JsonHelper.Serialize(ex.Message)
			};
		}
	}

	private async Task<int> HandleGetTotalVisitsInPeriod(string payload)
	{
		var period = JsonHelper.Deserialize<PeriodRequestDto>(payload);
		return await _analyticsService.GetTotalVisitsInPeriodAsync(period.Start, period.End);
	}
}
