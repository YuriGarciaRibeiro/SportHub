using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class EstablishmentUserService : IEstablishmentUserService
{
    private readonly IEstablishmentUsersRepository _establishmentUsersRepository;
    private readonly ICacheService _cacheService;

    public EstablishmentUserService(
        IEstablishmentUsersRepository establishmentUsersRepository,
        ICacheService cacheService)
    {
        _establishmentUsersRepository = establishmentUsersRepository;
        _cacheService = cacheService;
    }

    public async Task<List<Guid>> GetByOwnerIdAsync(Guid ownerId, CancellationToken ct = default)
    {
        var cacheKey = $"establishment-users-by-owner-{ownerId}";
        var cached = await _cacheService.GetAsync<List<Guid>>(cacheKey, ct);
        if (cached is not null) return cached;

        var result = await _establishmentUsersRepository.GetByOwnerIdAsync(ownerId, ct);
        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(30), ct);
        return result;
    }

    public async Task<EstablishmentUser?> GetAsync(Guid userId, Guid establishmentId, CancellationToken ct = default)
    {
        var cacheKey = $"establishment-user-{userId}-{establishmentId}";
        var cached = await _cacheService.GetAsync<EstablishmentUser>(cacheKey, ct);
        if (cached is not null) return cached;

        var result = await _establishmentUsersRepository.GetAsync(userId, establishmentId, ct);
        if (result is not null)
            await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(30), ct);
        return result;
    }

    public async Task<bool> HasRoleAnywhereAsync(Guid userId, EstablishmentRole requiredRole, CancellationToken ct = default)
    {
        var cacheKey = $"user-role-anywhere-{userId}-{requiredRole}";
        var cached = await _cacheService.GetAsync<string>(cacheKey, ct);
        if (cached is not null) return bool.Parse(cached);

        var result = await _establishmentUsersRepository.HasRoleAnywhereAsync(userId, requiredRole, ct);
        await _cacheService.SetAsync(cacheKey, result.ToString(), TimeSpan.FromMinutes(15), ct);
        return result;
    }

    public async Task AddManyAsync(IEnumerable<EstablishmentUser> establishmentUsers, CancellationToken ct = default)
    {
        await _establishmentUsersRepository.AddManyAsync(establishmentUsers, ct);
        
        // Clear relevant cache entries
        foreach (var establishmentUser in establishmentUsers)
        {
            await _cacheService.RemoveAsync($"establishment-user-{establishmentUser.UserId}-{establishmentUser.EstablishmentId}", ct);
            await _cacheService.RemoveAsync($"establishment-users-by-owner-{establishmentUser.UserId}", ct);
            await _cacheService.RemoveAsync($"user-role-anywhere-{establishmentUser.UserId}-{establishmentUser.Role}", ct);
        }
    }

    public async Task<EstablishmentUser> CreateAsync(EstablishmentUser entity, CancellationToken ct = default)
    {
        await _establishmentUsersRepository.AddAsync(entity, ct);
        
        // Clear relevant cache entries
        await _cacheService.RemoveAsync($"establishment-user-{entity.UserId}-{entity.EstablishmentId}", ct);
        await _cacheService.RemoveAsync($"establishment-users-by-owner-{entity.UserId}", ct);
        await _cacheService.RemoveAsync($"user-role-anywhere-{entity.UserId}-{entity.Role}", ct);
        
        return entity;
    }

    public async Task UpdateAsync(EstablishmentUser entity, CancellationToken ct = default)
    {
        await _establishmentUsersRepository.UpdateAsync(entity, ct);
        
        // Clear relevant cache entries
        await _cacheService.RemoveAsync($"establishment-user-{entity.UserId}-{entity.EstablishmentId}", ct);
        await _cacheService.RemoveAsync($"establishment-users-by-owner-{entity.UserId}", ct);
        await _cacheService.RemoveAsync($"user-role-anywhere-{entity.UserId}-{entity.Role}", ct);
    }
}
