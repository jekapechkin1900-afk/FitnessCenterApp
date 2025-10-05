using System.ComponentModel.DataAnnotations;

namespace FitnessCenterApp.AnalyticsService.Domain;

public class Visit
{
	public Guid Id { get; set; }

	[Required]
	public Guid ClientId { get; set; }

	public DateTime VisitTime { get; set; }
}