using Application.UseCases.Sport.CreateSport;
using Application.UseCases.Sport.DeleteSport;
using Application.UseCases.Sport.GetAllSports;
using Application.UseCases.Sport.UpdateSport;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebApi.Extensions.ResultExtensions;

namespace SportHub.Api.Endpoints;

public static class SportsEndpoints
{
    public static void MapSportsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/sports")
            .WithTags("Sports")
            .WithOpenApi();

        // GET /api/sports — público
        group.MapGet("/", async (ISender sender) =>
        {
            var result = await sender.Send(new GetAllSportsQuery());
            return result.ToIResult();
        })
        .WithName("GetAllSports")
        .WithSummary("Lista todos os esportes disponíveis")
        .AllowAnonymous()
        .Produces<List<SportSummaryResponse>>(StatusCodes.Status200OK);

        // POST /api/sports — autenticado
        group.MapPost("/", async (CreateSportCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess
                ? Results.Created($"/api/sports/{result.Value.Id}", result.Value)
                : result.ToIResult();
        })
        .WithName("CreateSport")
        .WithSummary("Cria um novo esporte no tenant atual")
        .RequireAuthorization()
        .Produces<CreateSportResponse>(StatusCodes.Status201Created)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status409Conflict)
        .Produces<ProblemDetails>(StatusCodes.Status422UnprocessableEntity);

        // PUT /api/sports/{id} — autenticado
        group.MapPut("/{id:guid}", async (Guid id, UpdateSportCommand command, ISender sender) =>
        {
            command.Id = id;
            var result = await sender.Send(command);
            return result.ToIResult();
        })
        .WithName("UpdateSport")
        .WithSummary("Atualiza um esporte existente")
        .RequireAuthorization()
        .Produces<UpdateSportResponse>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status422UnprocessableEntity);

        // DELETE /api/sports/{id} — autenticado
        group.MapDelete("/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new DeleteSportCommand { Id = id });
            return result.ToIResult();
        })
        .WithName("DeleteSport")
        .WithSummary("Remove um esporte")
        .RequireAuthorization()
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }
}
