using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class EstablishmentUserService : IEstablishmentUserService
{
    private readonly IEstablishmentUsersRepository _establishmentUsersRepository;
    private readonly ICacheService _cacheService;

    // TTLs específicos para diferentes tipos de dados
    private readonly TimeSpan _defaultTtl = TimeSpan.FromMinutes(30);
    private readonly TimeSpan _shortTtl = TimeSpan.FromMinutes(15);
    private readonly TimeSpan _roleCheckTtl = TimeSpan.FromMinutes(10);

    public EstablishmentUserService(
        IEstablishmentUsersRepository establishmentUsersRepository,
        ICacheService cacheService)
    {
        _establishmentUsersRepository = establishmentUsersRepository;
        _cacheService = cacheService;
    }

    public async Task<List<Guid>> GetByOwnerIdAsync(Guid ownerId, CancellationToken ct = default)
    {
        var cacheKey = _cacheService.GenerateCacheKey("EstablishmentUsersByOwner", ownerId.ToString());
        var cached = await _cacheService.GetAsync<List<Guid>>(cacheKey, ct);
        if (cached is not null) return cached;

        var result = await _establishmentUsersRepository.GetByOwnerIdAsync(ownerId, ct);
        await _cacheService.SetAsync(cacheKey, result, _defaultTtl, ct);
        return result;
    }

    public async Task<EstablishmentUser?> GetAsync(Guid userId, Guid establishmentId, CancellationToken ct = default)
    {
        var cacheKey = _cacheService.GenerateCacheKey("EstablishmentUser", $"{userId}_{establishmentId}");
        var cached = await _cacheService.GetAsync<EstablishmentUser>(cacheKey, ct);
        if (cached is not null) return cached;

        var result = await _establishmentUsersRepository.GetAsync(userId, establishmentId, ct);
        if (result is not null)
            await _cacheService.SetAsync(cacheKey, result, _defaultTtl, ct);
        return result;
    }

    public async Task<bool> HasRoleAnywhereAsync(Guid userId, EstablishmentRole requiredRole, CancellationToken ct = default)
    {
        var cacheKey = _cacheService.GenerateCacheKey("UserRoleCheck", $"{userId}_{requiredRole}");
        var cached = await _cacheService.GetAsync<string>(cacheKey, ct);
        if (cached is not null) return bool.Parse(cached);

        var result = await _establishmentUsersRepository.HasRoleAnywhereAsync(userId, requiredRole, ct);
        await _cacheService.SetAsync(cacheKey, result.ToString(), _roleCheckTtl, ct);
        return result;
    }

    public async Task AddManyAsync(IEnumerable<EstablishmentUser> establishmentUsers, CancellationToken ct = default)
    {
        await _establishmentUsersRepository.AddManyAsync(establishmentUsers, ct);
        
        // Invalidação mais eficiente e estruturada
        await InvalidateCacheForUsersAsync(establishmentUsers, ct);
    }

    public async Task<EstablishmentUser> CreateAsync(EstablishmentUser entity, CancellationToken ct = default)
    {
        await _establishmentUsersRepository.AddAsync(entity, ct);
        
        await InvalidateCacheForUserAsync(entity, ct);
        
        return entity;
    }

    public async Task UpdateAsync(EstablishmentUser entity, CancellationToken ct = default)
    {
        await _establishmentUsersRepository.UpdateAsync(entity, ct);
        
        await InvalidateCacheForUserAsync(entity, ct);
    }

    private async Task InvalidateCacheForUserAsync(EstablishmentUser entity, CancellationToken ct)
    {
        // Remove caches específicos desta relação
        var userEstablishmentKey = _cacheService.GenerateCacheKey("EstablishmentUser", $"{entity.UserId}_{entity.EstablishmentId}");
        var ownerKey = _cacheService.GenerateCacheKey("EstablishmentUsersByOwner", entity.UserId.ToString());
        var roleKey = _cacheService.GenerateCacheKey("UserRoleCheck", $"{entity.UserId}_{entity.Role}");

        await _cacheService.RemoveAsync(userEstablishmentKey, ct);
        await _cacheService.RemoveAsync(ownerKey, ct);
        await _cacheService.RemoveAsync(roleKey, ct);
    }

    private async Task InvalidateCacheForUsersAsync(IEnumerable<EstablishmentUser> entities, CancellationToken ct)
    {
        var tasks = entities.Select(entity => InvalidateCacheForUserAsync(entity, ct));
        await Task.WhenAll(tasks);
    }
}
