using Application.Security;
using Application.UseCases.Court.CreateCourt;
using Application.UseCases.Court.GetCourtsByEstablishmentId;
using Application.UseCases.Establishments.ActiveEstablishment;
using Application.UseCases.Establishments.CreateEstablishment;
using Application.UseCases.Establishments.DeleteEstablishment;
using Application.UseCases.Establishments.GetEstablishmentById;
using Application.UseCases.Establishments.GetEstablishments;
using Application.UseCases.Establishments.UpdateEstablishment;
using Application.UseCases.EstablishmentUser.CreateEstablishmentUser;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebApi.Extensions.ResultExtensions;

namespace SportHub.Api.Endpoints;

public static class EstablishmentsEndpoints
{
    public static void MapEstablishmentsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/establishments")
            .WithTags("Establishments");

        // GET /establishments - List all establishments
        group.MapGet("/", async (
            ISender sender,
            [FromQuery] string? ownerId,
            [FromQuery] string? isAvailable,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10) =>
        {
            Guid? parsedOwnerId = Guid.TryParse(ownerId, out var g) ? g : null;
            bool? parsedIsAvailable = bool.TryParse(isAvailable, out var b) ? b : null;

            var query = new GetEstablishmentsQuery(parsedOwnerId, parsedIsAvailable, page, pageSize);

            var result = await sender.Send(query);
            return result.ToIResult();
        })
        .WithName("GetEstablishments")
        .WithSummary("Get all establishments")
        .WithDescription("Retrieves a paginated list of establishments with optional filtering capabilities.")
        .Produces<GetEstablishmentsResponse>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
        .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
        .Produces<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        // POST /establishments - Create new establishment
        group.MapPost("/", async (
            CreateEstablishmentCommand command,
            ISender sender) =>
        {
            var result = await sender.Send(command);

            return result.ToIResult(StatusCodes.Status201Created);
        })
        .WithName("CreateEstablishment")
        .WithSummary("Create a new establishment")
        .WithDescription("Creates a new sports establishment with the provided details such as name, location, and contact information.")
        .Produces<string>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
        .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
        .Produces<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
        .RequireAuthorization();

        // PUT /establishments/{id} - Update establishment
        group.MapPut("/{establishmentId:guid}", async (
            Guid establishmentId,
            UpdateEstablishmentCommand command,
            ISender sender) =>
        {
            var result = await sender.Send(command with { Id = establishmentId });

            return result.ToIResult();
        })
        .WithName("UpdateEstablishment")
        .WithSummary("Update an establishment")
        .WithDescription("Updates the details of an existing establishment, including name, description, address, and image URL.")
        .Produces<UpdateEstablishmentResponse>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
        .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
        .RequireAuthorization(PolicyNames.IsEstablishmentOwner);

        // Delete /establishments/{id} - Delete establishment
        group.MapDelete("/{establishmentId:guid}", async (
            Guid establishmentId,
            ISender sender) =>
        {
            var result = await sender.Send(new DeleteEstablishmentCommand(establishmentId));

            return result.ToIResult();
        })
        .WithName("DeleteEstablishment")
        .WithSummary("Delete an establishment")
        .WithDescription("Deletes the specified establishment, removing it from the system and all associated data.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
        .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
        .RequireAuthorization(PolicyNames.IsEstablishmentOwner);

        // Post /establishments/{establishmentId}/activate - Activate establishment
        group.MapPost("/{establishmentId:guid}/activate", async (
            Guid establishmentId,
            ISender sender) =>
        {
            var result = await sender.Send(new ActiveEstablishmentCommand(establishmentId));

            return result.ToIResult();
        })
        .WithName("ActivateEstablishment")
        .WithSummary("Activate an establishment")
        .WithDescription("Restores the specified establishment, making it active again in the system.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
        .RequireAuthorization(PolicyNames.IsEstablishmentOwner);

        // GET /establishments/{id} - Get establishment by ID
        group.MapGet("/{id:guid}", async (
            Guid id,
            ISender sender) =>
        {
            var result = await sender.Send(new GetEstablishmentByIdQuery(id));

            return result.ToIResult();
        })
        .WithName("GetEstablishment")
        .WithSummary("Get establishment by ID")
        .WithDescription("Retrieves detailed information about a specific establishment using its unique identifier.")
        .Produces<GetEstablishmentByIdResponse>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        // POST /establishments/{establishmentId}/users - Associate user with establishment
        group.MapPost("/{establishmentId:guid}/users", async (
            Guid establishmentId,
            CreateEstablishmentUserRequest request,
            ISender sender) =>
        {
            var createCommand = new CreateEstablishmentUserCommand(request, establishmentId);

            var result = await sender.Send(createCommand);

            return result.ToIResult(StatusCodes.Status201Created);
        })
        .WithName("CreateEstablishmentUser")
        .WithSummary("Associate a user with an establishment")
        .WithDescription("Associates an existing user with the specified establishment, granting them appropriate roles and permissions within that establishment.")
        .Produces<CreateEstablishmentUserResponse>(StatusCodes.Status201Created)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
        .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
        .RequireAuthorization(PolicyNames.IsEstablishmentManager);

        // POST /establishments/{establishmentId}/courts - Create court
        group.MapPost("/{establishmentId}/courts", async (
            Guid establishmentId,
            CourtRequest request,
            ISender sender) =>
        {
            var result = await sender.Send(new CreateCourtCommand(establishmentId, request));

            return result.ToIResult();
        })
        .WithName("CreateCourt")
        .WithSummary("Create a new court")
        .WithDescription("Creates a new sports court within the specified establishment with details like name, sport type, and capacity.")
        .RequireAuthorization(PolicyNames.IsEstablishmentManager)
        .Produces(StatusCodes.Status204NoContent)
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
    }
}
