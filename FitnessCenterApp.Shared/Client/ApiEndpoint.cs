namespace FitnessCenterApp.Shared.Client;

public record ApiEndpoint(string Method, string Path, string Description, string ExampleBody = "");