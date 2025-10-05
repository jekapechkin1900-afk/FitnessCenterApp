namespace FitnessCenterApp.Shared.Contracts;

public record MembershipDto
{
	public Guid Id { get; init; }
	public Guid ClientId { get; set; }
	public DateTime StartDate { get; init; }
	public DateTime EndDate { get; init; }
	public string Type { get; init; } = string.Empty;
	public decimal Price { get; set; }
}
