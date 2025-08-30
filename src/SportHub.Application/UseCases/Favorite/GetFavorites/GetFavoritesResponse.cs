using Domain.Enums;

namespace Application.UseCases.Favorite.GetFavorites;

public class GetFavoritesResponse
{
    public List<FavoriteDto> Favorites { get; set; } = new();
}

public class FavoriteDto
{
    public Guid Id { get; set; }
    public FavoriteType EntityType { get; set; }
    public Guid EntityId { get; set; }
}
