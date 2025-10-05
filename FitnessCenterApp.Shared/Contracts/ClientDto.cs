namespace FitnessCenterApp.Shared.Contracts;

public record ClientDto
{
	public Guid Id { get; set; }
	public string FullName { get; set; } = string.Empty;
	public string? Email { get; set; }
	public string? PhoneNumber { get; set; }
}