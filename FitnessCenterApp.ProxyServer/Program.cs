using FitnessCenterApp.ProxyServer;

var gateway = new HttpGatewayServer("127.0.0.1", 8080);
await gateway.StartAsync();