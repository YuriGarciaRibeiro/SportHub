using Application.Security;
using Application.UseCases.Location.CreateLocation;
using Application.UseCases.Location.DeleteLocation;
using Application.UseCases.Location.GetAllLocations;
using Application.UseCases.Location.UpdateLocation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebApi.Extensions.ResultExtensions;

namespace SportHub.Api.Endpoints;

public static class LocationsEndpoints
{
    public static IEndpointRouteBuilder MapLocationsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/locations").WithTags("Locations");

        // GET /api/locations — público
        group.MapGet("/", async (ISender sender) =>
        {
            var result = await sender.Send(new GetAllLocationsQuery());
            return result.ToIResult();
        })
        .WithName("GetLocations")
        .WithSummary("Lista todas as unidades do tenant atual (público)")
        .AllowAnonymous()
        .Produces<List<LocationResponse>>(StatusCodes.Status200OK);

        // POST /api/locations — IsOwner
        group.MapPost("/", async (LocationRequest request, ISender sender) =>
        {
            var command = new CreateLocationCommand(request);
            var result = await sender.Send(command);
            return result.IsSuccess
                ? Results.Created($"/api/locations/{result.Value}", new { id = result.Value })
                : result.ToIResult();
        })
        .WithName("CreateLocation")
        .WithSummary("Cria uma nova unidade no tenant atual")
        .RequireAuthorization(PolicyNames.IsOwner)
        .Produces<object>(StatusCodes.Status201Created)
        .Produces<ProblemDetails>(StatusCodes.Status422UnprocessableEntity);

        // PUT /api/locations/{id} — IsOwner
        group.MapPut("/{id:guid}", async (Guid id, LocationRequest request, ISender sender) =>
        {
            var command = new UpdateLocationCommand { Id = id, Location = request };
            var result = await sender.Send(command);
            return result.ToIResult();
        })
        .WithName("UpdateLocation")
        .WithSummary("Atualiza os dados de uma unidade")
        .RequireAuthorization(PolicyNames.IsOwner)
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status422UnprocessableEntity);

        // DELETE /api/locations/{id} — IsOwner
        group.MapDelete("/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new DeleteLocationCommand(id));
            return result.ToIResult();
        })
        .WithName("DeleteLocation")
        .WithSummary("Remove uma unidade (soft delete)")
        .RequireAuthorization(PolicyNames.IsOwner)
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        return app;
    }
}
