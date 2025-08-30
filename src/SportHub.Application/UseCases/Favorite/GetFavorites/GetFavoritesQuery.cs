using Domain.Enums;

namespace Application.UseCases.Favorite.GetFavorites;

public class GetFavoritesQuery : IQuery<GetFavoritesResponse>
{
    public Guid UserId { get; set; }
    public FavoriteType? EntityType { get; set; }
}
