namespace FitnessCenterApp.Shared.Contracts;

public record MembershipDto
{
	public Guid Id { get; init; }
	public string ClientName { get; init; } = string.Empty;
	public DateTime StartDate { get; init; }
	public DateTime EndDate { get; init; }
	public string Type { get; init; } = string.Empty;
}
