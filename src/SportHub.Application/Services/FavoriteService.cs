using Application.Common.Interfaces.Favorites;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class FavoriteService : IFavoriteService
{
    private readonly IFavoriteRepository _favoriteRepository;
    private readonly ICourtService _courtService;
    private readonly IEstablishmentService _establishmentService;
    private readonly ISportService _sportService;
    private readonly ICacheService _cacheService;

    public FavoriteService(IFavoriteRepository favoriteRepository, ICourtService courtService, IEstablishmentService establishmentService, ISportService sportService, ICacheService cacheService)
    {
        _favoriteRepository = favoriteRepository;
        _courtService = courtService;
        _establishmentService = establishmentService;
        _sportService = sportService;
        _cacheService = cacheService;
    }

    public async Task<Result> AddFavoriteAsync(Guid userId, FavoriteType entityType, Guid entityId, CancellationToken cancellationToken)
    {
        // Validate if the entity exists
        bool entityExists = await ValidateEntityExists(entityType, entityId, cancellationToken);
        if (!entityExists)
        {
            return Result.Fail($"Entity of type {entityType} with ID {entityId} does not exist.");
        }

        var existing = await _favoriteRepository.GetByUserAndEntityAsync(userId, entityType, entityId, cancellationToken);
        if (existing != null)
        {
            return Result.Fail("Favorite already exists.");
        }

        var favorite = new Favorite
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            EntityType = entityType,
            EntityId = entityId
        };

        await _favoriteRepository.AddAsync(favorite, cancellationToken);
        var cacheKey = _cacheService.GenerateCacheKey("favorites", userId, entityType.ToString());
        var allCacheKey = _cacheService.GenerateCacheKey("favorites", userId, "all");
        await _cacheService.RemoveAsync(cacheKey);
        await _cacheService.RemoveAsync(allCacheKey);
        return Result.Ok();
    }

    private async Task<bool> ValidateEntityExists(FavoriteType entityType, Guid entityId, CancellationToken cancellationToken)
    {
        return entityType switch
        {
            FavoriteType.Court => await _courtService.GetByIdAsync(entityId, cancellationToken) != null,
            FavoriteType.Establishment => await _establishmentService.GetByIdAsync(entityId, cancellationToken) != null,
            FavoriteType.Sport => await _sportService.GetByIdAsync(entityId, cancellationToken) != null,
            _ => false
        };
    }

    public async Task<Result> RemoveFavoriteAsync(Guid userId, FavoriteType entityType, Guid entityId, CancellationToken cancellationToken)
    {
        var favorite = await _favoriteRepository.GetByUserAndEntityAsync(userId, entityType, entityId, cancellationToken);
        if (favorite == null)
        {
            return Result.Fail("Favorite not found.");
        }

        await _favoriteRepository.RemoveAsync(favorite, cancellationToken);
        var cacheKey = _cacheService.GenerateCacheKey("favorites", userId, entityType.ToString());
        var allCacheKey = _cacheService.GenerateCacheKey("favorites", userId, "all");
        await _cacheService.RemoveAsync(cacheKey);
        await _cacheService.RemoveAsync(allCacheKey);
        return Result.Ok();
    }

    public async Task<List<FavoriteDto>> GetFavoritesAsync(Guid userId, FavoriteType? entityType, CancellationToken cancellationToken)
    {
        var cacheKey = _cacheService.GenerateCacheKey("favorites", userId, entityType.HasValue ? entityType.Value.ToString() : "all");
        var cachedFavorites = await _cacheService.GetAsync<List<FavoriteDto>>(cacheKey);
        if (cachedFavorites != null)
        {
            return cachedFavorites;
        }

        var favorites = await _favoriteRepository.GetByUserAsync(userId, entityType, cancellationToken);
        await _cacheService.SetAsync(cacheKey, favorites, TimeSpan.FromMinutes(5));

        return favorites;
    }
}
