namespace FitnessCenterApp.ClientService.Application.Dtos;

public record MembershipDtoForManipulation(string ClientName, DateTime StartDate, DateTime EndDate, string Type, decimal Price);
