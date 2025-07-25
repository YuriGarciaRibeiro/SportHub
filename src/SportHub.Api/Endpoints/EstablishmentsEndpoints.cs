using Application.Security;
using Application.UserCases.Court.CreateCourt;
using Application.UserCases.Court.GetCourtsByEstablishmentId;
using Application.UserCases.Establishments.GetEstablishmentById;
using Application.UserCases.Establishments.GetEstablishmentByOwnerId;
using Application.UserCases.EstablishmentUser.CreateEstablishmentUser;
using MediatR;
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
        .RequireAuthorization();

        group.MapGet("/{id:guid}", async (
            Guid id,
            ISender sender) =>
        {
            var result = await sender.Send(new GetEstablishmentByIdQuery(id));

            return result.ToIResult();
        })
        .WithName("GetEstablishment")
        .WithSummary("Get an establishment by ID")
        .WithDescription("Retrieves an establishment by its ID.");

        group.MapGet("/owner", async (
            ISender sender) =>
        {
            var result = await sender.Send(new GetEstablishmentByOwnerIdQuery());

            return result.ToIResult();
        })
        .WithName("GetEstablishmentsByOwnerId")
        .WithSummary("Get establishments by owner ID")
        .WithDescription("Retrieves all establishments associated with a specific owner ID.")
        .RequireAuthorization(PolicyNames.IsOwner);

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
        .RequireAuthorization();

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
        .RequireAuthorization(PolicyNames.IsEstablishmentManager);


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
        .RequireAuthorization();
    }
}
