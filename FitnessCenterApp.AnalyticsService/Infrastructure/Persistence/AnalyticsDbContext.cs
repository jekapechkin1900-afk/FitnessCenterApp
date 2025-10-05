using FitnessCenterApp.AnalyticsService.Domain;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterApp.AnalyticsService.Infrastructure.Persistence;

public class AnalyticsDbContext : DbContext
{
	public DbSet<Visit> Visits { get; set; }

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		var dbPath = Path.Combine(AppContext.BaseDirectory, "clients_service.db");
		optionsBuilder.UseSqlite($"Data Source={dbPath}");
	}
}
