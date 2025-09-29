using System.Collections.Generic;
using System.Reflection.Emit;
using FitnessCenterApp.ClientService.Domain;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterApp.ClientService.Infrastructure.Persistence;

public class ClientDbContext : DbContext
{
	public DbSet<Membership> Memberships { get; set; }

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		var dbPath = Path.Combine(AppContext.BaseDirectory, "clients.db");
		optionsBuilder.UseSqlite($"Data Source={dbPath}");
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		var membershipEntity = modelBuilder.Entity<Membership>();
		membershipEntity.HasKey(m => m.Id);
		membershipEntity.Property(m => m.Id).ValueGeneratedNever();
	}
}
