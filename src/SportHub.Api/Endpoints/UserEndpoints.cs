using Api.Extensions.Results;
using Application.UseCases.Favorite.GetFavorites;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SportHub.Application.UseCases.Reservation.GetReservationByUserId;

namespace SportHub.Api.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/users")
            .WithTags("Users")
            .RequireAuthorization();

        // GET /users/favorites  -> favoritos do usuário autenticado
        group.MapGet("/favorites", async (
            [FromQuery] FavoriteType? entityType,
            ISender sender,
            ICurrentUserService currentUser,
            HttpContext httpContext
        ) =>
        {
            var query = new GetFavoritesQuery
            {
                UserId = currentUser.UserId,
                EntityType = entityType
            };

            var result = await sender.Send(query, httpContext.RequestAborted);
            return result.ToIResult();
        })
        .WithName("GetFavorites")
        .WithSummary("Get favorites")
        .WithDescription("Returns the list of favorites for the current user.")
        .Produces<IEnumerable<GetFavoritesResponse>>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
        .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        // GET /users/reservations
        group.MapGet("/reservations", async (
            ISender sender,
            ICurrentUserService currentUser,
            HttpContext httpContext,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10
        ) =>
        {
            var query = new GetReservationsByUserIdQuery
            {
                UserId = currentUser.UserId,
                Page = page,
                PageSize = pageSize
            };

            var result = await sender.Send(query, httpContext.RequestAborted);
            return result.ToIResult();
        })
        .WithName("GetUserReservations")
        .WithSummary("Get user reservations")
        .WithDescription("Returns the list of reservations for the current user.")
        .Produces<IEnumerable<GetReservationsByUserIdResponse>>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
        .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }
}

