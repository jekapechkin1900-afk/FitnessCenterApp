using System.Net.Sockets;
using System.Text;

namespace FitnessCenterApp.Shared.Http;

public static class HttpRequestParser
{
	public static async Task<ParsedHttpRequest> ParseAsync(NetworkStream stream)
	{
		var request = new ParsedHttpRequest();
		var buffer = new byte[8192];
		var bytesRead = await stream.ReadAsync(buffer);
		var requestData = Encoding.UTF8.GetString(buffer, 0, bytesRead);

		var headerEndIndex = requestData.IndexOf("\r\n\r\n");
		if (headerEndIndex == -1)
		{
			throw new InvalidOperationException("Invalid HTTP request: Headers not found.");
		}

		var headerData = requestData[..headerEndIndex];
		var lines = headerData.Split("\r\n");

		var requestLine = lines[0].Split(' ');
		request.Method = requestLine[0];
		var fullPath = requestLine[1];

		var queryIndex = fullPath.IndexOf('?');
		if (queryIndex != -1)
		{
			request.Path = fullPath[..queryIndex];
			request.Query = fullPath[(queryIndex + 1)..];
		}
		else
		{
			request.Path = fullPath;
			request.Query = string.Empty;
		}

		for (int i = 1; i < lines.Length; i++)
		{
			var headerLine = lines[i];
			var delimiterIndex = headerLine.IndexOf(':');
			if (delimiterIndex != -1)
			{
				var name = headerLine[..delimiterIndex].Trim();
				var value = headerLine[(delimiterIndex + 1)..].Trim();
				request.Headers[name] = value;
			}
		}

		if (request.Headers.TryGetValue("Content-Length", out var contentLengthStr)
			&& int.TryParse(contentLengthStr, out var contentLength)
			&& contentLength > 0)
		{
			var bodyStartIndex = headerEndIndex + 4; 
			var initialBodyLength = bytesRead - bodyStartIndex;

			var bodyBuilder = new StringBuilder();
			bodyBuilder.Append(requestData, bodyStartIndex, initialBodyLength);

			var remainingBytes = contentLength - initialBodyLength;
			if (remainingBytes > 0)
			{
				var bodyBuffer = new byte[remainingBytes];
				await stream.ReadExactlyAsync(bodyBuffer, 0, remainingBytes);
				bodyBuilder.Append(Encoding.UTF8.GetString(bodyBuffer));
			}
			request.Body = bodyBuilder.ToString();
		}
		else
		{
			request.Body = string.Empty;
		}

		return request;
	}
}