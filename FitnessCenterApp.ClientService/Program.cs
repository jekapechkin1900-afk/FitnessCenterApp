using FitnessCenterApp.ClientService;

var server = new ClientServiceHost(9001, "Сервис Клиентов и Абонементов");
server.Start();
