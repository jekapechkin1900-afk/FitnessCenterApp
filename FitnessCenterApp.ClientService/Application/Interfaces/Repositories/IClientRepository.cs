using FitnessCenterApp.ClientService.Domain;

namespace FitnessCenterApp.ClientService.Application.Interfaces.Repositories;

public interface IClientRepository
{
	Task<Client?> GetByIdAsync(Guid id);
	Task<IEnumerable<Client>> GetAllAsync();
	Task<Client> AddAsync(Client client);
	Task UpdateAsync(Client client);
	Task DeleteAsync(Guid id);
	Task<bool> ExistsAsync(Guid id);
}
