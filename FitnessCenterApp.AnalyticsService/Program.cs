using FitnessCenterApp.AnalyticsService;
using FitnessCenterApp.AnalyticsService.Application.Interfaces.Repositories;
using FitnessCenterApp.AnalyticsService.Application.Interfaces.Services;
using FitnessCenterApp.AnalyticsService.Application.Services;
using FitnessCenterApp.AnalyticsService.Infrastructure.Persistence;
using FitnessCenterApp.AnalyticsService.Infrastructure.Persistence.Repositories;
using FitnessCenterApp.Shared.Clients;

var contextFactory = new Func<AnalyticsDbContext>(() => new AnalyticsDbContext());

IVisitRepository visitRepository = new VisitRepository(contextFactory);

var clientServiceRpcClient = new RpcClient("127.0.0.1", 9001);

IAnalyticsService analyticsService = new AnalyticsService(visitRepository, clientServiceRpcClient);

var server = new RpcServer("127.0.0.1", 9002, analyticsService);

await server.StartAsync();