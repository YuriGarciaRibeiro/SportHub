using Application.Common.Enums;
using Application.Common.QueryFilters;
using Application.UseCases.Establishments.GetEstablishments;
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

    public async Task<(List<EstablishmentResponse> Items, int TotalCount)> GetFilteredAsync(GetEstablishmentsQuery query, CancellationToken cancellationToken)
    {
        // Direct call without cache due to complex return type
        return await _establishmentRepository.GetFilteredAsync(query, cancellationToken);
    }

    public async Task<Establishment?> GetByIdWithAddressAsync(Guid id, CancellationToken ct = default)
    {
        var key = _cache.GenerateCacheKey(CacheKeyPrefix.EntityById, nameof(Establishment), "withAddress", id);
        var cached = await _cache.GetAsync<Establishment>(key, ct);
        if (cached is not null) return cached;

        var establishment = await _establishmentRepository.GetByIdWithAddressAsync(id, ct);
        if (establishment is not null)
            await _cache.SetAsync(key, establishment, DefaultTtl, ct);
        
        return establishment;
    }

    public async Task<List<User>> GetUsersByEstablishmentIdAsync(Guid establishmentId, CancellationToken ct = default)
    {
        var key = _cache.GenerateCacheKey(CacheKeyPrefix.EntityById, nameof(Establishment), "users", establishmentId);
        var cached = await _cache.GetAsync<List<User>>(key, ct);
        if (cached is not null) return cached;

        var users = await _establishmentRepository.GetUsersByEstablishmentId(establishmentId, ct);
        await _cache.SetAsync(key, users, TimeSpan.FromMinutes(30), ct);
        
        return users;
    }

    public async Task<List<Sport>> GetSportsByEstablishmentIdAsync(Guid establishmentId, CancellationToken ct = default)
    {
        var key = _cache.GenerateCacheKey(CacheKeyPrefix.EntityById, nameof(Establishment), "sports", establishmentId);
        var cached = await _cache.GetAsync<List<Sport>>(key, ct);
        if (cached is not null) return cached;

        var sports = await _establishmentRepository.GetSportsByEstablishmentIdAsync(establishmentId, ct);
        await _cache.SetAsync(key, sports, TimeSpan.FromMinutes(60), ct);
        
        return sports;
    }

    public async Task<List<Reservation>> GetReservationsByCourtsIdAsync(IEnumerable<Guid> courtIds, EstablishmentReservationsQueryFilter filter, CancellationToken ct = default)
    {
        // Due to complex filter parameters, direct repository call without cache
        return await _establishmentRepository.GetReservationsByCourtsIdAsync(courtIds, filter, ct);
    }
}