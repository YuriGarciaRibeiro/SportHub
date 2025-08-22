using Application.Security;
using Application.UseCases.Establishments.GetEstablishmentUsers;
using Application.UseCases.Establishments.UpdateEstablishmentUserRole;
using Application.UseCases.EstablishmentUser.CreateEstablishmentUser;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebApi.Extensions.ResultExtensions;

namespace SportHub.Api.Endpoints.Establishments;

public static class EstablishmentUsersEndpoints
{
    public static RouteGroupBuilder MapEstablishmentUsersEndpoints(this RouteGroupBuilder group)
    {
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

        // GET /establishments/{establishmentId}/users - Get users by establishment ID
        group.MapGet("/{establishmentId:guid}/users", async (
            Guid establishmentId,
            ISender sender) =>
        {
            var result = await sender.Send(new GetEstablishmentUsersQuery { EstablishmentId = establishmentId });

            return result.ToIResult();
        })
        .WithName("GetEstablishmentUsers")
        .WithSummary("Get users by establishment ID")
        .WithDescription("Retrieves a list of users associated with the specified establishment, including their roles and permissions.")
        .Produces<GetEstablishmentUsersResponse>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
        .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
        .RequireAuthorization(PolicyNames.IsEstablishmentManager);

        // PUT /establishments/{establishmentId}/users/{userId}/role - Update user role in establishment
        group.MapPut("/{establishmentId:guid}/users/{userId:guid}/role", async (
            Guid establishmentId,
            Guid userId,
            [FromBody] UpdateEstablishmentUserRoleRequest request,
            ISender sender) =>
        {
            var command = new UpdateEstablishmentUserRoleCommand
            {
                EstablishmentId = establishmentId,
                UserId = userId,
                Request = request
            };

            var result = await sender.Send(command);

            return result.ToIResult();
        })
        .WithName("UpdateEstablishmentUserRole")
        .WithSummary("Update user role in establishment")
        .WithDescription("Updates the role of a user within the specified establishment, allowing for role changes such as staff or manager.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
        .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
        .RequireAuthorization(PolicyNames.IsEstablishmentManager);

        return group;
    }
}