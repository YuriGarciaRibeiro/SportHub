using Application.Security;
using Application.UseCases.Court.CreateCourt;
using Application.UseCases.Court.GetCourtsByEstablishmentId;
using Application.UseCases.Court.UpdateCourt;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Api.Extensions.Results;

namespace SportHub.Api.Endpoints.Establishments;

public static class EstablishmentCourtsEndpoints
{
    public static RouteGroupBuilder MapEstablishmentCourtsEndpoints(this RouteGroupBuilder group)
    {
        // POST /establishments/{establishmentId}/courts - Create court
        group.MapPost("/{establishmentId:guid}/courts", async (
            Guid establishmentId,
            CourtRequest request,
            ISender sender) =>
        {
            var result = await sender.Send(new CreateCourtCommand(establishmentId, request));

            return result.ToIResult(StatusCodes.Status201Created);
        })
        .WithName("CreateCourt")
        .WithSummary("Create a new court")
        .WithDescription("Creates a new sports court within the specified establishment with details like name, sport type, and capacity.")
        .RequireAuthorization(PolicyNames.IsEstablishmentManager)
        .Produces(StatusCodes.Status201Created)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
        .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        // GET /establishments/{establishmentId}/courts - Get courts by establishment
        group.MapGet("/{establishmentId:guid}/courts", async (
            Guid establishmentId,
            ISender sender) =>
        {
            var result = await sender.Send(new GetCourtsByEstablishmentIdQuery(establishmentId));

            return result.ToIResult();
        })
        .WithName("GetCourtsByEstablishmentId")
        .WithSummary("Get courts by establishment ID")
        .WithDescription("Retrieves all sports courts associated with a specific establishment, including their details and availability status.")
        .RequireAuthorization()
        .Produces<GetCourtsByEstablishmentIdResponse>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
        .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        // DELETE /establishments/{establishmentId}/courts/{courtId}
        group.MapDelete("/{establishmentId:guid}/courts/{courtId:guid}", async (
            Guid establishmentId,
            Guid courtId,
            ISender sender) =>
        {
            var command = new DeleteCourtCommand
            {
                Id = courtId
            };

            var result = await sender.Send(command);

            return result.ToIResult();
        })
        .WithName("DeleteCourt")
        .WithSummary("Delete a court")
        .WithDescription("Deletes a specific court from the establishment, removing it from the system and all associated data.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
        .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
        .RequireAuthorization(PolicyNames.IsEstablishmentManager);

        // PUT /establishments/{establishmentId}/courts/{courtId} - Update court
        group.MapPut("/{establishmentId:guid}/courts/{courtId:guid}",
            async (
            Guid establishmentId,
            Guid courtId,
            UpdateCourtRequest request,
            ISender sender) =>
        {
            var command = new UpdateCourtCommand
            {
                Id = courtId,
                EstablishmentId = establishmentId,
                Request = request
            };

            var result = await sender.Send(command);

            return result.ToIResult();
        })
        .WithName("UpdateCourt")
        .WithSummary("Update a court")
        .WithDescription("Updates the details of an existing court within the specified establishment.")
        .Produces<UpdateCourtResponse>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
        .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
        .RequireAuthorization(PolicyNames.IsEstablishmentManager);

        return group;
    }
}
