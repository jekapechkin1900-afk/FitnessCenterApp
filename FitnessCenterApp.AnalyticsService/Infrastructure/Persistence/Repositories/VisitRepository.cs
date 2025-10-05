using FitnessCenterApp.AnalyticsService.Application.Interfaces.Repositories;
using FitnessCenterApp.AnalyticsService.Domain;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterApp.AnalyticsService.Infrastructure.Persistence.Repositories;

public class VisitRepository(Func<AnalyticsDbContext> contextFactory) : IVisitRepository
{
	private readonly Func<AnalyticsDbContext> _contextFactory = contextFactory;

	public async Task<Visit> AddAsync(Visit visit)
	{
		using var context = _contextFactory();
		context.Visits.Add(visit);
		await context.SaveChangesAsync();
		return visit;
	}

	public async Task<IEnumerable<Visit>> GetAllAsync()
	{
		using var context = _contextFactory();
		return await context.Visits.AsNoTracking().ToListAsync();
	}

	public async Task<int> GetCountInPeriodAsync(DateTime start, DateTime end)
	{
		using var context = _contextFactory();
		return await context.Visits.CountAsync(v => v.VisitTime >= start && v.VisitTime <= end);
	}

	public async Task<Visit> GetByIdAsync(Guid id)
	{
		using var context = _contextFactory();
		return await context.Visits.FindAsync(id);
	}

	public async Task UpdateAsync(Visit visit)
	{
		using var context = _contextFactory();
		context.Visits.Update(visit);
		await context.SaveChangesAsync();
	}

	public async Task DeleteAsync(Guid id)
	{
		using var context = _contextFactory();
		var visit = await context.Visits.FindAsync(id);
		if (visit != null)
		{
			context.Visits.Remove(visit);
			await context.SaveChangesAsync();
		}
	}
}
