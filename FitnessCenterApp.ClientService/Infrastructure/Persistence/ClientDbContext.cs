using System.Collections.Generic;
using System.Reflection.Emit;
using FitnessCenterApp.ClientService.Domain;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterApp.ClientService.Infrastructure.Persistence;

public class ClientDbContext : DbContext
{
	public DbSet<Client> Clients { get; set; }
	public DbSet<Membership> Memberships { get; set; }

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		var dbPath = Path.Combine(AppContext.BaseDirectory, "clients_service.db");
		optionsBuilder.UseSqlite($"Data Source={dbPath}");
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<Client>()
		   .HasIndex(c => c.Email)
		   .IsUnique();
	}
}
