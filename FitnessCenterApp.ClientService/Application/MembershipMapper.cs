using FitnessCenterApp.ClientService.Application.Dtos;
using FitnessCenterApp.ClientService.Domain;
using FitnessCenterApp.Shared.Contracts;

namespace FitnessCenterApp.ClientService.Application;

public static class MembershipMapper
{
	public static Membership ToEntity(this MembershipDtoForManipulation dto)
	{
		return new Membership
		{
			Id = Guid.NewGuid(),
			ClientName = dto.ClientName,
			StartDate = dto.StartDate,
			EndDate = new DateTime(dto.EndDate.Year, dto.EndDate.Month, dto.EndDate.Day, 23, 59, 59),
			Type = dto.Type
		};
	}

	public static MembershipDto ToDto(this Membership entity)
	{
		return new MembershipDto
		{
			Id = entity.Id,
			ClientName = entity.ClientName,
			StartDate = entity.StartDate,
			EndDate = entity.EndDate,
			Type = entity.Type
		};
	}
}
