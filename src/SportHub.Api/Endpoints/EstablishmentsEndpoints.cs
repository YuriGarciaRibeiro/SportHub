using Application.Security;
using Application.UserCases.Court.CreateCourt;
using Application.UserCases.Court.GetCourtsByEstablishmentId;
using Application.UserCases.Establishments.GetEstablishmentById;
using Application.UserCases.Establishments.GetEstablishmentByOwnerId;
using Application.UserCases.EstablishmentUser.CreateEstablishmentUser;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebApi.Extensions.ResultExtensions;

public static class EstablishmentsEndpoints
{
    public static void MapEstablishmentsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/establishments")
            .WithTags("Establishments");



        group.MapPost("/", async (
            CreateEstablishmentCommand command,
            ISender sender) =>
        {
            var result = await sender.Send(command);

            return result.ToIResult();
        })
        .WithName("CreateEstablishment")
        .WithSummary("Create a new establishment")
        .WithDescription("Creates a new establishment with the provided details.")
        .RequireAuthorization()
        .Produces<string>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
        .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
        .Produces<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        group.MapGet("/{id:guid}", async (
            Guid id,
            ISender sender) =>
        {
            var result = await sender.Send(new GetEstablishmentByIdQuery(id));

            return result.ToIResult();
        })
        .WithName("GetEstablishment")
        .WithSummary("Get an establishment by ID")
        .WithDescription("Retrieves an establishment by its ID.")
        .Produces<GetEstablishmentByIdResponse>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        group.MapGet("/owner", async (
            ISender sender) =>
        {
            var result = await sender.Send(new GetEstablishmentByOwnerIdQuery());

            return result.ToIResult();
        })
        .WithName("GetEstablishmentsByOwnerId")
        .WithSummary("Get establishments by owner ID")
        .WithDescription("Retrieves all establishments associated with a specific owner ID.")
        .RequireAuthorization(PolicyNames.IsOwner)
        .Produces<GetEstablishmentsByOwnerIdResponse>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
        .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        group.MapPost("/users", async (
            CreateEstablishmentUserCommand command,
            ISender sender) =>
        {
            var result = await sender.Send(command);

            return result.ToIResult();
        })
        .WithName("CreateEstablishmentUser")
        .WithSummary("Create a user for an establishment")
        .WithDescription("Creates a new user for the specified establishment.")
        .RequireAuthorization()
        .Produces<CreateEstablishmentUserResponse>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
        .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        group.MapPost("/{establishmentId:guid}/courts", async (
            Guid establishmentId,
            CourtRequest request,
            ISender sender) =>
        {
            var result = await sender.Send(new CreateCourtCommand(establishmentId, request));

            return result.ToIResult();
        })
        .WithName("CreateCourt")
        .WithSummary("Create a new court")
        .WithDescription("Creates a new court for the specified establishment.")
        .RequireAuthorization(PolicyNames.IsEstablishmentManager)
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
        .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);


        group.MapGet("/{establishmentId:guid}/courts", async (
            Guid establishmentId,
            ISender sender) =>
        {
            var result = await sender.Send(new GetCourtsByEstablishmentIdQuery(establishmentId));

            return result.ToIResult();
        })
        .WithName("GetCourtsByEstablishmentId")
        .WithSummary("Get courts by establishment ID")
        .WithDescription("Retrieves all courts associated with a specific establishment ID.")
        .RequireAuthorization()
        .Produces<GetCourtsByEstablishmentIdResponse>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
        .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }
}
