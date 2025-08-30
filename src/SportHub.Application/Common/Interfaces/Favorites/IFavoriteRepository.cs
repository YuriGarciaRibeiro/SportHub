using Domain.Entities;
using Domain.Enums;

namespace Application.Common.Interfaces.Favorites;

public interface IFavoriteRepository
{
    Task<Favorite?> GetByUserAndEntityAsync(Guid userId, FavoriteType entityType, Guid entityId, CancellationToken cancellationToken);
    Task<List<FavoriteDto>> GetByUserAsync(Guid userId, FavoriteType? entityType, CancellationToken cancellationToken);
    Task AddAsync(Favorite favorite, CancellationToken cancellationToken);
    Task RemoveAsync(Favorite favorite, CancellationToken cancellationToken);
}
