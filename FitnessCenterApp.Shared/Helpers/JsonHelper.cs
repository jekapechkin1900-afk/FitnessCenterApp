using System.Text.Json;
using System.Text.Json.Serialization;

namespace FitnessCenterApp.Shared.Helpers;

public static class JsonHelper
{
	private static readonly JsonSerializerOptions Options = new()
	{
		PropertyNameCaseInsensitive = true,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
	};

	public static string Serialize<T>(T obj)
	{
		return JsonSerializer.Serialize(obj, Options);
	}

	public static T? Deserialize<T>(string json)
	{
		return JsonSerializer.Deserialize<T>(json, Options);
	}
}
