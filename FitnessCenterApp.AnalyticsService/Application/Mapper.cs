using FitnessCenterApp.AnalyticsService.Domain;
using FitnessCenterApp.Shared.Contracts;

namespace FitnessCenterApp.AnalyticsService.Application;

public static class Mapper
{
	public static VisitDto ToDto(this Visit entity) => new()
	{
		Id = entity.Id,
		ClientId = entity.ClientId,
		VisitTime = entity.VisitTime,
	};
}
