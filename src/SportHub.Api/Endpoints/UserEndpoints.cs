using Api.Extensions.Results;
using Application.UseCases.Favorite.GetFavorites;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

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
    }
}

