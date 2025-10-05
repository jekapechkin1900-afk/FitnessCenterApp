using FitnessCenterApp.AnalyticsService.Domain;

namespace FitnessCenterApp.AnalyticsService.Application.Interfaces.Repositories;

public interface IVisitRepository
{
	Task<Visit> AddAsync(Visit visit);
	Task<Visit> GetByIdAsync(Guid id); 
	Task<IEnumerable<Visit>> GetAllAsync();
	Task UpdateAsync(Visit visit); 
	Task DeleteAsync(Guid id); 
	Task<int> GetCountInPeriodAsync(DateTime start, DateTime end);
}