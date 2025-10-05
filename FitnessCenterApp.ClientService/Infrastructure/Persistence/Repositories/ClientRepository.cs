using FitnessCenterApp.ClientService.Application.Interfaces.Repositories;
using FitnessCenterApp.ClientService.Domain;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterApp.ClientService.Infrastructure.Persistence.Repositories;

public class ClientRepository : IClientRepository
{
	private readonly Func<ClientDbContext> _contextFactory;

	public ClientRepository(Func<ClientDbContext> contextFactory)
	{
		_contextFactory = contextFactory;
	}

	public async Task<Client> GetByIdAsync(Guid id)
	{
		using var context = _contextFactory();
		return await context.Clients.FindAsync(id);
	}

	public async Task<IEnumerable<Client>> GetAllAsync()
	{
		using var context = _contextFactory();
		return await context.Clients.AsNoTracking().ToListAsync();
	}

	public async Task<Client> AddAsync(Client client)
	{
		using var context = _contextFactory();
		context.Clients.Add(client);
		await context.SaveChangesAsync();
		return client;
	}

	public async Task UpdateAsync(Client client)
	{
		using var context = _contextFactory();
		context.Clients.Update(client);
		await context.SaveChangesAsync();
	}

	public async Task DeleteAsync(Guid id)
	{
		using var context = _contextFactory();
		var client = await context.Clients.FindAsync(id);
		if (client != null)
		{
			context.Clients.Remove(client);
			await context.SaveChangesAsync();
		}
	}

	public async Task<bool> ExistsAsync(Guid id)
	{
		using var context = _contextFactory();
		return await context.Clients.AnyAsync(c => c.Id == id);
	}
}