using FitnessCenterApp.ClientService.Domain;

namespace FitnessCenterApp.ClientService.Application.Interfaces.Repositories;

public interface IMembershipRepository
{
	Task<Membership?> GetByIdAsync(Guid id);
	Task<IEnumerable<Membership>> GetAllAsync();
	Task<IEnumerable<Membership>> GetByClientIdAsync(Guid clientId);
	Task<Membership> AddAsync(Membership membership);
	Task UpdateAsync(Membership membership);
	Task DeleteAsync(Guid id);
}
