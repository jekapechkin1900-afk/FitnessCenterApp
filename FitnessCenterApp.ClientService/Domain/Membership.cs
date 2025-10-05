using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessCenterApp.ClientService.Domain;

public class Membership
{
	public Guid Id { get; set; }

	[Required]
	public Guid ClientId { get; set; }

	[ForeignKey(nameof(ClientId))]
	public Client? Client { get; set; }

	public DateTime StartDate { get; set; }
	public DateTime EndDate { get; set; }

	[Required]
	public string Type { get; set; } = string.Empty;

	[Column(TypeName = "decimal(18, 2)")]
	public decimal Price { get; set; }
}
