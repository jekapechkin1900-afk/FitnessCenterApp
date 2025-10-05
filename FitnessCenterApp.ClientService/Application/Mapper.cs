using FitnessCenterApp.ClientService.Application.Dtos;
using FitnessCenterApp.ClientService.Domain;
using FitnessCenterApp.Shared.Contracts;

namespace FitnessCenterApp.ClientService.Application;

public static class Mapper
{
	public static Membership ToEntity(this MembershipDtoForManipulation dto) => new()
	{
		Id = Guid.NewGuid(),
		StartDate = dto.StartDate,
		EndDate = new DateTime(dto.EndDate.Year, dto.EndDate.Month, dto.EndDate.Day, 23, 59, 59),
		Type = dto.Type,
		Price = dto.Price,
	};

	public static MembershipDto ToDto(this Membership entity) => new()
	{
		Id = entity.Id,
		ClientId = entity.ClientId,
		StartDate = entity.StartDate,
		EndDate = entity.EndDate,
		Type = entity.Type,
		Price = entity.Price,
	};

	public static ClientDto ToDto(this Client client) => new()
	{
		Id = client.Id,
		FullName = client.FullName,
		Email = client.Email,
		PhoneNumber = client.PhoneNumber
	};
}
