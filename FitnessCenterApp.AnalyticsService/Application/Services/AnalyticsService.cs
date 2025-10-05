using FitnessCenterApp.AnalyticsService.Application.Interfaces.Repositories;
using FitnessCenterApp.AnalyticsService.Application.Interfaces.Services;
using FitnessCenterApp.AnalyticsService.Domain;
using FitnessCenterApp.Shared.Clients;
using FitnessCenterApp.Shared.Contracts;
using FitnessCenterApp.Shared.Helpers;
using FitnessCenterApp.Shared.RPC;

namespace FitnessCenterApp.AnalyticsService.Application.Services;

public class AnalyticsService(IVisitRepository visitRepository, RpcClient clientServiceRpcClient) : IAnalyticsService
{
	private readonly IVisitRepository _visitRepository = visitRepository;
	private readonly RpcClient _clientServiceRpcClient = clientServiceRpcClient;

	public async Task<VisitDto> CreateVisitAsync(VisitDto visitDto)
	{
		var visit = new Visit
		{
			Id = Guid.NewGuid(),
			ClientId = visitDto.ClientId,
			VisitTime = DateTime.UtcNow
		};
		var createdVisit = await _visitRepository.AddAsync(visit);

		return new VisitDto
		{
			Id = createdVisit.Id,
			ClientId = createdVisit.ClientId,
			VisitTime = createdVisit.VisitTime
		};
	}

	public async Task<Guid?> GetMostActiveClientIdAsync()
	{
		var visits = await _visitRepository.GetAllAsync();
		if (!visits.Any()) return null;

		var result = visits
			.GroupBy(v => v.ClientId)
			.Select(g => new { ClientId = g.Key, Count = g.Count() })
			.OrderByDescending(x => x.Count)
			.FirstOrDefault();

		return result?.ClientId;
	}

	public async Task<DayOfWeek?> GetBusiestDayOfWeekAsync()
	{
		var visits = await _visitRepository.GetAllAsync();
		if (!visits.Any()) return null;

		var result = visits
			.GroupBy(v => v.VisitTime.DayOfWeek)
			.Select(g => new { Day = g.Key, Count = g.Count() })
			.OrderByDescending(x => x.Count)
			.First();

		return result.Day;
	}

	public async Task<object> GetMostProfitableMonthAsync()
	{
		var memberships = await GetAllMembershipsFromClientServiceAsync();
		if (memberships.Count == 0) return null;

		var result = memberships
			.GroupBy(m => new { m.StartDate.Year, m.StartDate.Month })
			.Select(g => new
			{
				g.Key.Year,
				g.Key.Month,
				TotalRevenue = g.Sum(m => m.Price)
			})
			.OrderByDescending(x => x.TotalRevenue)
			.FirstOrDefault();

		return result;
	}

	public async Task<string> GetMostPopularMembershipTypeAsync()
	{
		var memberships = await GetAllMembershipsFromClientServiceAsync();
		if (memberships.Count == 0) return "N/A";

		var result = memberships
			.GroupBy(m => m.Type)
			.Select(g => new { Type = g.Key, Count = g.Count() })
			.OrderByDescending(x => x.Count)
			.FirstOrDefault();

		return result?.Type;
	}

	public async Task<int> GetTotalVisitsInPeriodAsync(DateTime start, DateTime end)
	{
		return await _visitRepository.GetCountInPeriodAsync(start, end);
	}

	private async Task<List<MembershipDto>> GetAllMembershipsFromClientServiceAsync()
	{
		var request = new RpcRequest { ServiceName = "ClientService", MethodName = "GetAllMemberships", Payload = "" };
		var response = await _clientServiceRpcClient.SendRequestAsync(request);

		if (response.StatusCode != RpcStatusCode.Ok)
		{
			throw new Exception($"Failed to get memberships from ClientService. Status: {response.StatusCode}, Payload: {response.Payload}");
		}

		return JsonHelper.Deserialize<List<MembershipDto>>(response.Payload) ?? new List<MembershipDto>();
	}

	public async Task<VisitDto?> GetVisitByIdAsync(Guid id)
	{
		var visit = await _visitRepository.GetByIdAsync(id);
		return visit?.ToDto();
	}

	public async Task<IEnumerable<VisitDto>> GetAllVisitsAsync()
	{
		var visits = await _visitRepository.GetAllAsync();
		return visits.Select(c => c.ToDto());
	}

	public async Task<VisitDto> UpdateVisitAsync(VisitDto visitDto)
	{
		var existingClient = await _visitRepository.GetByIdAsync(visitDto.Id)
			?? throw new KeyNotFoundException($"Client with ID {visitDto.Id} not found.");

		existingClient.ClientId = visitDto.ClientId;
		existingClient.VisitTime = visitDto.VisitTime;

		await _visitRepository.UpdateAsync(existingClient);
		return existingClient.ToDto();
	}

	public async Task<bool> DeleteVisitAsync(Guid id)
	{
		var client = await _visitRepository.GetByIdAsync(id);
		if (client == null) return false;

		await _visitRepository.DeleteAsync(id);
		return true;
	}
}
