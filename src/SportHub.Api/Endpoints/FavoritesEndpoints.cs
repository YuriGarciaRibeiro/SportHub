using Application.UseCases.Favorite.AddFavorite;
using Application.UseCases.Favorite.GetFavorites;
using Application.UseCases.Favorite.RemoveFavorite;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Api.Extensions.Results;

namespace SportHub.Api.Endpoints;

public static class FavoritesEndpoints
{
    public static void MapFavoritesEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/favorites")
            .WithTags("Favorites")
            .RequireAuthorization();

        // POST /favorites - Adicionar favorito
        group.MapPost("/", async (ISender sender, [FromBody] AddFavoriteRequest request, ICurrentUserService currentUser) =>
        {
            var command = new AddFavoriteCommand
            {
                UserId = currentUser.UserId,
                EntityType = request.EntityType,
                EntityId = request.EntityId
            };

            var result = await sender.Send(command);
            return result.ToIResult();
        })
        .WithName("AddFavorite")
        .WithSummary("Add a favorite")
        .WithDescription("Adds a favorite for the current user.")
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        // DELETE /favorites/{entityType}/{entityId} - Remover favorito
        group.MapDelete("/{entityType}/{entityId:guid}", async (ISender sender, FavoriteType entityType, Guid entityId, ICurrentUserService currentUser) =>
        {
            var command = new RemoveFavoriteCommand
            {
                UserId = currentUser.UserId,
                EntityType = entityType,
                EntityId = entityId
            };

            var result = await sender.Send(command);
            return result.ToIResult();
        })
        .WithName("RemoveFavorite")
        .WithSummary("Remove a favorite")
        .WithDescription("Removes a favorite for the current user.")
        .Produces(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }
}

public class AddFavoriteRequest
{
    public FavoriteType EntityType { get; set; }
    public Guid EntityId { get; set; }
}
