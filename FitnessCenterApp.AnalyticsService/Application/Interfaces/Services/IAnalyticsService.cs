using FitnessCenterApp.Shared.Contracts;

namespace FitnessCenterApp.AnalyticsService.Application.Interfaces.Services;

public interface IAnalyticsService
{
	Task<Guid?> GetMostActiveClientIdAsync();
	Task<DayOfWeek?> GetBusiestDayOfWeekAsync();
	Task<object> GetMostProfitableMonthAsync();
	Task<string> GetMostPopularMembershipTypeAsync();
	Task<int> GetTotalVisitsInPeriodAsync(DateTime start, DateTime end);

	Task<VisitDto> CreateVisitAsync(VisitDto visitDto);
	Task<VisitDto?> GetVisitByIdAsync(Guid id);
	Task<IEnumerable<VisitDto>> GetAllVisitsAsync();
	Task<VisitDto> UpdateVisitAsync(VisitDto visitDto);
	Task<bool> DeleteVisitAsync(Guid id);
}
