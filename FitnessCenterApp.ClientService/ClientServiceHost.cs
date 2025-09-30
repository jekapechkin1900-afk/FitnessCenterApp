using System.Text.Json;
using System.Text.RegularExpressions;
using FitnessCenterApp.ClientService.Application;
using FitnessCenterApp.ClientService.Application.Dtos;
using FitnessCenterApp.ClientService.Infrastructure.Persistence.Repositories;
using FitnessCenterApp.Shared.Client;
using FitnessCenterApp.Shared.Constants;
using FitnessCenterApp.Shared.Http;

namespace FitnessCenterApp.ClientService;

internal class ClientServiceHost(int port, string serviceName) : ApiServiceBase(port, serviceName)
{
	private static readonly MembershipRepository _repository = new();

	protected override async Task RouteApiRequest(Stream stream, HttpRequest request)
	{
		var membershipByIdPattern = new Regex(@"^/api/memberships/([0-9a-fA-F\-]{36})$");
		var match = membershipByIdPattern.Match(request.Path);

		Console.WriteLine($"==> Обработка API: Метод='{request.Method}', Путь='{request.Path}', Тело='{request.Body?.Length ?? 0} байт'");
		if (request.Path == "/api/memberships" && request.Method == "GET")
		{
			var dtos = _repository.GetAll().Select(m => m.ToDto());
			await WriteJsonResponse(stream, HttpCodes.Ok, dtos);
		}
		else if (request.Path == "/api/memberships" && request.Method == "POST")
		{
			var dto = JsonSerializer.Deserialize<MembershipDtoForManipulation>(request.Body, _jsonOptions);
			var createdEntity = _repository.Create(dto.ToEntity());
			await WriteJsonResponse(stream, HttpCodes.Created, createdEntity.ToDto());
		}
		else if (match.Success) 
		{
			var id = Guid.Parse(match.Groups[1].Value);

			switch (request.Method)
			{
				case "GET":
					var entity = _repository.GetById(id);
					if (entity != null)
					{
						await WriteJsonResponse(stream, HttpCodes.Ok, entity.ToDto());
					}
					else
					{
						await WriteJsonResponse(stream, HttpCodes.NotFound, new { error = $"Membership with ID {id} not found." });
					}
					break;

				case "PUT":
					var dtoToUpdate = JsonSerializer.Deserialize<MembershipDtoForManipulation>(request.Body, _jsonOptions);
					var updatedEntity = _repository.Update(id, dtoToUpdate.ToEntity());
					if (updatedEntity != null)
					{
						await WriteJsonResponse(stream, HttpCodes.Ok, updatedEntity.ToDto());
					}
					else
					{
						await WriteJsonResponse(stream, HttpCodes.NotFound, new { error = $"Membership with ID {id} not found." });
					}
					break;

				case "DELETE":
					var success = _repository.Delete(id);
					if (success)
					{
						await WriteHttpResponse(stream, HttpCodes.NoContent, "application/json", Array.Empty<byte>());
					}
					else
					{
						await WriteJsonResponse(stream, HttpCodes.NotFound, new { error = $"Membership with ID {id} not found." });
					}
					break;

				default:
					await WriteJsonResponse(stream, HttpCodes.MethodNotAllowed, new { error = "Method not allowed for this endpoint." });
					break;
			}
		}
		else
		{
			await WriteJsonResponse(stream, "404 Not Found", new { error = "Endpoint not found" });
		}
	}

	protected override ApiEndpoint[] GetApiEndpoints()
	{
		var exampleBody = "{\n  \"clientName\": \"Иван Обновленный\",\n  \"startDate\": \"2024-01-01\",\n  \"endDate\": \"2025-01-01\",\n  \"type\": \"Вечерний\"\n}";

		return
		[
			new ApiEndpoint("GET", "/api/memberships", "Получить все абонементы"),
			new ApiEndpoint("POST", "/api/memberships", "Создать новый абонемент", 
				exampleBody.Replace("Иван Обновленный", "Иван Новый").Replace("Вечерний", "Полный день")),

			new ApiEndpoint("GET", "/api/memberships/{id}", "Получить абонемент по ID"),
			new ApiEndpoint("PUT", "/api/memberships/{id}", "Обновить абонемент по ID", exampleBody),
			new ApiEndpoint("DELETE", "/api/memberships/{id}", "Удалить абонемент по ID")
		];
	}
}
