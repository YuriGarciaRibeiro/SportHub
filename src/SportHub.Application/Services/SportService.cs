using Application.Common.Enums;
using Application.Common.Interfaces;
using Domain.Entities;

namespace Application.Services;

public class SportService : BaseService<Sport>, ISportService
{
    private readonly ISportsRepository _sportRepository;

    protected override TimeSpan DefaultTtl => TimeSpan.FromMinutes(60); // Sports don't change frequently

    public SportService(
        ISportsRepository sportRepository,
        ICacheService cacheService)
        : base(sportRepository, cacheService)
    {
        _sportRepository = sportRepository;
    }

    public async Task<List<Sport>> GetSportsByEstablishmentIdAsync(Guid establishmentId, CancellationToken ct = default)
    {
        // For now, return all sports - this would need to be implemented in the repository
        // based on the relationship between establishments and sports
        return await GetAllAsync(ct: ct);
    }

    public async Task<Sport?> GetSportByNameAsync(string name, CancellationToken ct = default)
    {
        var key = _cache.GenerateCacheKey(CacheKeyPrefix.Query, nameof(Sport), "byName", name);
        var cached = await _cache.GetAsync<Sport>(key, ct);
        if (cached is not null) return cached;

        var sport = await _sportRepository.GetByNameAsync(name, ct);
        if (sport is not null)
            await _cache.SetAsync(key, sport, DefaultTtl, ct);
        
        return sport;
    }

    public async Task<List<Sport>> GetSportsByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
    {
        var idsList = ids.ToList();
        var key = _cache.GenerateCacheKey(CacheKeyPrefix.Query, nameof(Sport), "byIds", string.Join(",", idsList));
        var cached = await _cache.GetAsync<List<Sport>>(key, ct);
        if (cached is not null) return cached;

        var sports = await _sportRepository.GetSportsByIdsAsync(idsList, ct);
        var sportsList = sports.ToList();
        await _cache.SetAsync(key, sportsList, DefaultTtl, ct);
        
        return sportsList;
    }
}
