namespace FitnessCenterApp.ClientService.Domain;

public class Membership
{
	public Guid Id { get; set; }
	public string ClientName { get; set; }
	public DateTime StartDate { get; set; }
	public DateTime EndDate { get; set; }
	public string Type { get; set; }
}
