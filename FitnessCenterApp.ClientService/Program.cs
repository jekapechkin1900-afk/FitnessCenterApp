using FitnessCenterApp.ClientService;
using FitnessCenterApp.ClientService.Application.Interfaces.Repositories;
using FitnessCenterApp.ClientService.Application.Interfaces.Services;
using FitnessCenterApp.ClientService.Application.Services;
using FitnessCenterApp.ClientService.Infrastructure.Persistence;
using FitnessCenterApp.ClientService.Infrastructure.Persistence.Repositories;

var contextFactory = new Func<ClientDbContext>(() => new ClientDbContext());

IClientRepository clientRepository = new ClientRepository(contextFactory);
IMembershipRepository membershipRepository = new MembershipRepository(contextFactory);

IClientService clientService = new ClientService(clientRepository);
IMembershipService membershipService = new MembershipService(membershipRepository, clientRepository);

var server = new RpcServer("127.0.0.1", 9001, clientService, membershipService);

await server.StartAsync();
