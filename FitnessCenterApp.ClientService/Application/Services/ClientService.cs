using FitnessCenterApp.ClientService.Application.Interfaces.Repositories;
using FitnessCenterApp.ClientService.Application.Interfaces.Services;
using FitnessCenterApp.ClientService.Domain;
using FitnessCenterApp.Shared.Contracts;

namespace FitnessCenterApp.ClientService.Application.Services;

public class ClientService(IClientRepository clientRepository) : IClientService
{
	private readonly IClientRepository _clientRepository = clientRepository;

	public async Task<ClientDto> CreateClientAsync(ClientDto clientDto)
	{
		var client = new Client
		{
			Id = Guid.NewGuid(),
			FullName = clientDto.FullName,
			Email = clientDto.Email,
			PhoneNumber = clientDto.PhoneNumber
		};
		var createdClient = await _clientRepository.AddAsync(client);
		return createdClient.ToDto();
	}

	public async Task<ClientDto?> GetClientByIdAsync(Guid id)
	{
		var client = await _clientRepository.GetByIdAsync(id);
		return client?.ToDto();
	}

	public async Task<IEnumerable<ClientDto>> GetAllClientsAsync()
	{
		var clients = await _clientRepository.GetAllAsync();
		return clients.Select(c => c.ToDto());
	}

	public async Task<ClientDto> UpdateClientAsync(ClientDto clientDto)
	{
		var existingClient = await _clientRepository.GetByIdAsync(clientDto.Id) 
			?? throw new KeyNotFoundException($"Client with ID {clientDto.Id} not found.");

		existingClient.FullName = clientDto.FullName;
		existingClient.Email = clientDto.Email;
		existingClient.PhoneNumber = clientDto.PhoneNumber;

		await _clientRepository.UpdateAsync(existingClient);
		return existingClient.ToDto();
	}

	public async Task<bool> DeleteClientAsync(Guid id)
	{
		var client = await _clientRepository.GetByIdAsync(id);
		if (client == null) return false;

		await _clientRepository.DeleteAsync(id);
		return true;
	}
}
