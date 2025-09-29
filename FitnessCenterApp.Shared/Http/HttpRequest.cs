namespace FitnessCenterApp.Shared.Http;

public class HttpRequest
{
	public string? Method { get; init; }
	public string? Path { get; init; }
	public string? Body { get; init; }
	public Dictionary<string, string> Headers { get; init; } = [];
}
