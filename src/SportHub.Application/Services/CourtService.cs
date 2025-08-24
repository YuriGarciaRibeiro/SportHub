using Application.Common.Interfaces;
using Application.Common.QueryFilters;
using Domain.Entities;

namespace Application.Services;

public class CourtService : BaseService<Court>, ICourtService
{
    private readonly ICourtsRepository _courtRepository;

    protected override TimeSpan DefaultTtl => TimeSpan.FromMinutes(30);

    public CourtService(
        ICourtsRepository courtRepository,
        ICacheService cacheService)
        : base(courtRepository, cacheService)
    {
        _courtRepository = courtRepository;
    }

    public async Task<List<Court>> GetCourtsByEstablishmentIdAsync(Guid establishmentId, CancellationToken ct = default)
    {
        var key = _cache.GenerateCacheKey("CourtsByEstablishment", establishmentId.ToString());
        var cached = await _cache.GetAsync<List<Court>>(key, ct);
        if (cached is not null) return cached;

        var courts = await _courtRepository.GetByEstablishmentIdAsync(establishmentId, ct);
        var courtsList = courts.ToList();
        await _cache.SetAsync(key, courtsList, DefaultTtl, ct);
        return courtsList;
    }

    public async Task<List<Guid>> GetCourtIdsByEstablishmentIdAsync(Guid establishmentId, CancellationToken ct = default)
    {
        var key = _cache.GenerateCacheKey("CourtIds", establishmentId.ToString());
        var cached = await _cache.GetAsync<List<Guid>>(key, ct);
        if (cached is not null) return cached;

        var courtIds = (await _courtRepository.GetCourtIdsByEstablishmentIdAsync(establishmentId, ct)).ToList();
        await _cache.SetAsync(key, courtIds, TimeSpan.FromHours(1), ct); // Cache longo para IDs
        
        return courtIds;
    }

    public async Task<List<Court>> GetAvailableCourtsAsync(Guid establishmentId, DateTime startTime, DateTime endTime, CancellationToken ct = default)
    {
        return await GetCourtsByEstablishmentIdAsync(establishmentId, ct);
    }

    public async Task<Court?> GetCompleteByIdAsync(Guid id, CancellationToken ct = default)
    {
        var key = _cache.GenerateCacheKey("CourtComplete", id.ToString());
        var cached = await _cache.GetAsync<Court>(key, ct);
        if (cached is not null) return cached;

        var court = await _courtRepository.GetCompleteByIdAsync(id, ct);
        if (court is not null)
            await _cache.SetAsync(key, court, DefaultTtl, ct);
        
        return court;
    }

    public async Task<List<Court>> GetByFilterAsync(CourtQueryFilter filter, CancellationToken ct = default)
    {
        var courts = await _courtRepository.GetByFilterAsync(filter, ct);
        return courts.ToList();
    }
}
