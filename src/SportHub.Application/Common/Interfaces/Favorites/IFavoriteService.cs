using Domain.Entities;
using Domain.Enums;

namespace Application.Common.Interfaces.Favorites;

public interface IFavoriteService
{
    Task<Result> AddFavoriteAsync(Guid userId, FavoriteType entityType, Guid entityId, CancellationToken cancellationToken);
    Task<Result> RemoveFavoriteAsync(Guid userId, FavoriteType entityType, Guid entityId, CancellationToken cancellationToken);
    Task<List<FavoriteDto>> GetFavoritesAsync(Guid userId, FavoriteType? entityType, CancellationToken cancellationToken);
}
