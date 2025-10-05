namespace FitnessCenterApp.Shared.Contracts;

public record VisitDto
{
	public Guid Id { get; set; }
	public Guid ClientId { get; set; }
	public DateTime VisitTime { get; set; }
}