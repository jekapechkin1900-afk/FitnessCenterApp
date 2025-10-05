namespace FitnessCenterApp.Shared.Http;

public class ParsedHttpRequest
{
	public string? Method { get; set; }
	public string? Path { get; set; }
	public string? Query { get; set; }
	public Dictionary<string, string> Headers { get; set; } = new(StringComparer.OrdinalIgnoreCase);
	public string? Body { get; set; }
}