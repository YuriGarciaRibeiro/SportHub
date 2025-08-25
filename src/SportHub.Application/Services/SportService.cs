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

    public async Task<IEnumerable<SportSummaryDto>> GetSportsByEstablishmentIdAsync(Guid establishmentId, CancellationToken ct = default)
    {
        return await _sportRepository.GetByEstablishmentIdAsync(establishmentId, ct);
    }

    public async Task<Sport?> GetSportByNameAsync(string name, CancellationToken ct = default)
    {
        return await _sportRepository.GetByNameAsync(name, ct);
    }
}
