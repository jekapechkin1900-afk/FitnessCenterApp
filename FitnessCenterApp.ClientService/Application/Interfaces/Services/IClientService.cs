using FitnessCenterApp.Shared.Contracts;

namespace FitnessCenterApp.ClientService.Application.Interfaces.Services;

public interface IClientService
{
	Task<ClientDto?> GetClientByIdAsync(Guid id);
	Task<IEnumerable<ClientDto>> GetAllClientsAsync();
	Task<ClientDto> CreateClientAsync(ClientDto clientDto);
	Task<ClientDto> UpdateClientAsync(ClientDto clientDto);
	Task<bool> DeleteClientAsync(Guid id);
}
