using FitnessCenterApp.Shared.Contracts;

namespace FitnessCenterApp.ClientService.Application.Interfaces.Services;

public interface IMembershipService
{
	Task<MembershipDto?> GetMembershipByIdAsync(Guid id);
	Task<IEnumerable<MembershipDto>> GetAllMembershipsAsync();
	Task<IEnumerable<MembershipDto>> GetMembershipsByClientIdAsync(Guid clientId);
	Task<MembershipDto> AddMembershipAsync(MembershipDto membershipDto);
	Task<MembershipDto> UpdateMembershipAsync(MembershipDto membershipDto);
	Task<bool> DeleteMembershipAsync(Guid id);
}