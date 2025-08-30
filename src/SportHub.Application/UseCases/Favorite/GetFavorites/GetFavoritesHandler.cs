using Application.Common.Interfaces.Favorites;
using Application.CQRS;
using Domain.Entities;

namespace Application.UseCases.Favorite.GetFavorites;

public class GetFavoritesHandler : IQueryHandler<GetFavoritesQuery, GetFavoritesResponse>
{
    private readonly IFavoriteService _favoriteService;

    public GetFavoritesHandler(IFavoriteService favoriteService)
    {
        _favoriteService = favoriteService;
    }

    public async Task<Result<GetFavoritesResponse>> Handle(GetFavoritesQuery request, CancellationToken cancellationToken)
    {
        var favorites = await _favoriteService.GetFavoritesAsync(request.UserId, request.EntityType, cancellationToken);
        var favoriteDtos = favorites.Select(f => new FavoriteDto
        {
            Id = f.Id,
            EntityType = f.EntityType,
            EntityId = f.EntityId,
        }).ToList();

        var response = new GetFavoritesResponse { Favorites = favoriteDtos };
        return Result.Ok(response);
    }
}
