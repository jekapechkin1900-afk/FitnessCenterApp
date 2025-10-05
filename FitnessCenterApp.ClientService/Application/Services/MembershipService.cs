using FitnessCenterApp.ClientService.Application.Interfaces.Repositories;
using FitnessCenterApp.ClientService.Application.Interfaces.Services;
using FitnessCenterApp.ClientService.Domain;
using FitnessCenterApp.Shared.Contracts;

namespace FitnessCenterApp.ClientService.Application.Services;

public class MembershipService(IMembershipRepository membershipRepository, IClientRepository clientRepository) : IMembershipService
{
	private readonly IMembershipRepository _membershipRepository = membershipRepository;
	private readonly IClientRepository _clientRepository = clientRepository; 

	public async Task<MembershipDto> AddMembershipAsync(MembershipDto membershipDto)
	{
		if (!await _clientRepository.ExistsAsync(membershipDto.ClientId))
		{
			throw new ArgumentException($"Client with ID {membershipDto.ClientId} does not exist.");
		}

		var membership = new Membership
		{
			Id = Guid.NewGuid(),
			ClientId = membershipDto.ClientId,
			StartDate = membershipDto.StartDate,
			EndDate = membershipDto.EndDate,
			Type = membershipDto.Type,
			Price = membershipDto.Price
		};

		var createdMembership = await _membershipRepository.AddAsync(membership);
		return createdMembership.ToDto();
	}

	public async Task<MembershipDto?> GetMembershipByIdAsync(Guid id)
	{
		var membership = await _membershipRepository.GetByIdAsync(id);
		return membership?.ToDto();
	}

	public async Task<IEnumerable<MembershipDto>> GetAllMembershipsAsync()
	{
		var memberships = await _membershipRepository.GetAllAsync();
		return memberships.Select(m => m.ToDto());
	}

	public async Task<IEnumerable<MembershipDto>> GetMembershipsByClientIdAsync(Guid clientId)
	{
		var memberships = await _membershipRepository.GetByClientIdAsync(clientId);
		return memberships.Select(m => m.ToDto());
	}

	public async Task<MembershipDto> UpdateMembershipAsync(MembershipDto membershipDto)
	{
		var existingMembership = await _membershipRepository.GetByIdAsync(membershipDto.Id) 
			?? throw new KeyNotFoundException($"Membership with ID {membershipDto.Id} not found.");
		
		existingMembership.ClientId = membershipDto.ClientId;
		existingMembership.StartDate = membershipDto.StartDate;
		existingMembership.EndDate = membershipDto.EndDate;
		existingMembership.Type = membershipDto.Type;
		existingMembership.Price = membershipDto.Price;

		await _membershipRepository.UpdateAsync(existingMembership);
		return existingMembership.ToDto();
	}

	public async Task<bool> DeleteMembershipAsync(Guid id)
	{
		var membership = await _membershipRepository.GetByIdAsync(id);
		if (membership == null) return false;

		await _membershipRepository.DeleteAsync(id);
		return true;
	}
}
