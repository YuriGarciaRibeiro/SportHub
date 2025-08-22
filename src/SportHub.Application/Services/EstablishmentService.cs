using Application.Common.Enums;
using Domain.Entities;

namespace Application.Services;

public class EstablishmentService : BaseService<Establishment>, IEstablishmentService
{
    private readonly IEstablishmentsRepository _establishmentRepository;
    private readonly IEstablishmentUsersRepository _establishmentUsersRepository;

    protected override TimeSpan DefaultTtl => TimeSpan.FromMinutes(30);

    public EstablishmentService(
        IEstablishmentsRepository establishmentRepository,
        IEstablishmentUsersRepository establishmentUsersRepository,
        ICacheService cacheService)
        : base(establishmentRepository, cacheService)
    {
        _establishmentUsersRepository = establishmentUsersRepository;
        _establishmentRepository = establishmentRepository;
    }


    public async Task<Result<List<Establishment>>> GetEstablishmentsByOwnerIdAsync(Guid ownerId, CancellationToken ct)
    {

        var key = _cache.GenerateCacheKey(CacheKeyPrefix.Query, nameof(Establishment), "byOwner", ownerId);
        var cached = await _cache.GetAsync<List<Establishment>>(key, ct);
        if (cached is not null) return Result.Ok(cached);

        var ids = await _establishmentUsersRepository.GetByOwnerIdAsync(ownerId, ct);
        var list = !ids?.Any() ?? true
            ? new List<Establishment>()
            : await _establishmentRepository.GetByIdsWithDetailsAsync(ids!, ct);

        await _cache.SetAsync(key, list, DefaultTtl, ct);
        return Result.Ok(list);

    }
}