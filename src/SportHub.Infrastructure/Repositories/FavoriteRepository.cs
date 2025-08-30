using Application.Common.Interfaces.Favorites;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class FavoriteRepository : IFavoriteRepository
{
    private readonly ApplicationDbContext _context;

    public FavoriteRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Favorite?> GetByUserAndEntityAsync(Guid userId, FavoriteType entityType, Guid entityId, CancellationToken cancellationToken)
    {
        return await _context.Favorites
            .Where(f => f.UserId == userId && f.EntityType == entityType && f.EntityId == entityId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<FavoriteDto>> GetByUserAsync(Guid userId, FavoriteType? entityType, CancellationToken cancellationToken)
    {
        return await _context.Favorites
            .Where(f => f.UserId == userId && (!entityType.HasValue || f.EntityType == entityType.Value))
            .AsNoTracking()
            .Select(f => new FavoriteDto
            {
                Id = f.Id,
                EntityType = f.EntityType,
                EntityId = f.EntityId,
            })
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Favorite favorite, CancellationToken cancellationToken)
    {
        await _context.Favorites.AddAsync(favorite, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAsync(Favorite favorite, CancellationToken cancellationToken)
    {
        _context.Favorites.Remove(favorite);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
