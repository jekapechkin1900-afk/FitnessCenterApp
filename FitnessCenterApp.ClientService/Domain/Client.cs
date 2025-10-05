using System.ComponentModel.DataAnnotations;

namespace FitnessCenterApp.ClientService.Domain;

public class Client
{
	public Guid Id { get; set; }

	[Required]
	[MaxLength(100)]
	public string FullName { get; set; } = string.Empty;

	[Required]
	[EmailAddress]
	public string? Email { get; set; }

	public string? PhoneNumber { get; set; }	

	public ICollection<Membership> Memberships { get; set; } = [];
}
