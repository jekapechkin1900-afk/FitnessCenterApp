using FitnessCenterApp.ClientService.Application.Interfaces.Repositories;
using FitnessCenterApp.ClientService.Domain;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterApp.ClientService.Infrastructure.Persistence.Repositories;

public class MembershipRepository(Func<ClientDbContext> contextFactory) : IMembershipRepository
{
	private readonly Func<ClientDbContext> _contextFactory = contextFactory;

	public async Task<Membership?> GetByIdAsync(Guid id)
	{
		using var context = _contextFactory();
		return await context.Memberships.FindAsync(id);
	}

	public async Task<IEnumerable<Membership>> GetAllAsync()
	{
		using var context = _contextFactory();
		return await context.Memberships.AsNoTracking().ToListAsync();
	}

	public async Task<IEnumerable<Membership>> GetByClientIdAsync(Guid clientId)
	{
		using var context = _contextFactory();
		return await context.Memberships
			.Where(m => m.ClientId == clientId)
			.AsNoTracking()
			.ToListAsync();
	}

	public async Task<Membership> AddAsync(Membership membership)
	{
		using var context = _contextFactory();
		context.Memberships.Add(membership);
		await context.SaveChangesAsync();
		return membership;
	}

	public async Task UpdateAsync(Membership membership)
	{
		using var context = _contextFactory();
		context.Memberships.Update(membership);
		await context.SaveChangesAsync();
	}

	public async Task DeleteAsync(Guid id)
	{
		using var context = _contextFactory();
		var membership = await context.Memberships.FindAsync(id);
		if (membership != null)
		{
			context.Memberships.Remove(membership);
			await context.SaveChangesAsync();
		}
	}
}
