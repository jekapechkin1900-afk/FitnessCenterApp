using System.Text;

namespace FitnessCenterApp.Shared.Http;

public static class HttpParser
{
	private static readonly byte[] HeaderSeparator = { 13, 10, 13, 10 };

	public static async Task<HttpRequest?> ParseRequest(Stream stream)
	{
		using var memoryStream = new MemoryStream();
		var buffer = new byte[1024]; // Читаем по 1 KB за раз
		int headerEndPosition = -1;
		int bytesRead;

		// 1. Читаем из сетевого потока, пока не найдем конец заголовков
		while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
		{
			memoryStream.Write(buffer, 0, bytesRead);
			var receivedData = memoryStream.ToArray();
			headerEndPosition = FindHeaderEnd(receivedData);
			if (headerEndPosition != -1)
			{
				break;
			}
			// Защита от слишком больших заголовков
			if (memoryStream.Length > 8192) return null;
		}

		if (headerEndPosition == -1) return null;

		var allBytesInMemory = memoryStream.ToArray();

		// 2. Парсим заголовки
		var headerBytes = allBytesInMemory.AsSpan(0, headerEndPosition).ToArray();
		var headerText = Encoding.UTF8.GetString(headerBytes);
		var headerLines = headerText.Split(new[] { "\r\n" }, StringSplitOptions.None);

		var requestLineParts = headerLines[0].Split(' ');
		var headers = new Dictionary<string, string>();
		foreach (var line in headerLines.Skip(1))
		{
			var headerParts = line.Split(':', 2);
			if (headerParts.Length == 2)
			{
				headers[headerParts[0].Trim().ToLower()] = headerParts[1].Trim();
			}
		}

		// 3. Аккуратно извлекаем тело запроса
		string body = null;
		if (headers.TryGetValue("content-length", out var contentLengthValue) &&
			int.TryParse(contentLengthValue, out var contentLength) &&
			contentLength > 0)
		{
			var bodyBuffer = new byte[contentLength];

			// Определяем, сколько байт тела мы уже прочитали вместе с заголовками
			int bodyStartPosition = headerEndPosition + HeaderSeparator.Length;
			int initialBodyBytesCount = allBytesInMemory.Length - bodyStartPosition;

			// Копируем уже прочитанную часть
			Array.Copy(allBytesInMemory, bodyStartPosition, bodyBuffer, 0, initialBodyBytesCount);

			// Дочитываем оставшуюся часть тела, если необходимо
			int remainingBytes = contentLength - initialBodyBytesCount;
			if (remainingBytes > 0)
			{
				int totalBytesReadForBody = initialBodyBytesCount;
				while (totalBytesReadForBody < contentLength)
				{
					int read = await stream.ReadAsync(bodyBuffer, totalBytesReadForBody, contentLength - totalBytesReadForBody);
					if (read == 0) break; // Поток закончился
					totalBytesReadForBody += read;
				}
			}
			body = Encoding.UTF8.GetString(bodyBuffer);
		}

		return new HttpRequest
		{
			Method = requestLineParts[0],
			Path = requestLineParts[1],
			Headers = headers,
			Body = body
		};
	}

	private static int FindHeaderEnd(byte[] data)
	{
		for (int i = 0; i < data.Length - 3; i++)
		{
			if (data[i] == HeaderSeparator[0] && data[i + 1] == HeaderSeparator[1] && data[i + 2] == HeaderSeparator[2] && data[i + 3] == HeaderSeparator[3])
			{
				return i;
			}
		}
		return -1;
	}
}
