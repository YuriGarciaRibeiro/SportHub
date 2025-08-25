using Application.Common.Interfaces.Base;
using Application.Common.Interfaces.Establishments;
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
        var key = _cache.GenerateCacheKey("EstablishmentsByOwner", ownerId.ToString());
        var cached = await _cache.GetAsync<List<Establishment>>(key, ct);
        if (cached is not null) return Result.Ok(cached);

        var ids = await _establishmentUsersRepository.GetByOwnerIdAsync(ownerId, ct);
        var dtoList = !ids?.Any() ?? true
            ? new List<Establishment>()
            : await _establishmentRepository.GetByIdsAsync(ids!, ct);

        // Convert to entities for legacy compatibility
        var list = dtoList.Select(dto => new Establishment 
        { 
            Id = dto.Id, 
            Name = dto.Name, 
            Description = dto.Description, 
            ImageUrl = dto.ImageUrl 
        }).ToList();

        await _cache.SetAsync(key, list, DefaultTtl, ct);
        return Result.Ok(list);
    }

    public async Task<(List<EstablishmentResponse> Items, int TotalCount)> GetFilteredAsync(GetEstablishmentsQuery query, CancellationToken cancellationToken)
    {
        return await _establishmentRepository.GetFilteredAsync(query, cancellationToken);
    }

    public async Task<EstablishmentWithAddressDto?> GetByIdWithAddressAsync(Guid id, CancellationToken ct = default)
    {
        var key = _cache.GenerateCacheKey("EstablishmentWithAddress", id.ToString());
        var cached = await _cache.GetAsync<EstablishmentWithAddressDto>(key, ct);
        if (cached is not null) return cached;

        var establishment = await _establishmentRepository.GetByIdWithAddressAsync(id, ct);
        if (establishment is not null)
            await _cache.SetAsync(key, establishment, DefaultTtl, ct);
        
        return establishment;
    }

    public async Task<List<EstablishmentUserSummaryDto>> GetUsersByEstablishmentIdAsync(Guid establishmentId, CancellationToken ct = default)
    {
        var key = _cache.GenerateCacheKey("EstablishmentUsers", establishmentId.ToString());
        var cached = await _cache.GetAsync<List<EstablishmentUserSummaryDto>>(key, ct);
        if (cached is not null) return cached;

        var users = await _establishmentRepository.GetUsersByEstablishmentId(establishmentId, ct);
        await _cache.SetAsync(key, users, TimeSpan.FromMinutes(15), ct);
        
        return users;
    }

    public async Task<List<SportSummaryDto>> GetSportsByEstablishmentIdAsync(Guid establishmentId, CancellationToken ct = default)
    {
        var key = _cache.GenerateCacheKey("EstablishmentSports", establishmentId.ToString());
        var cached = await _cache.GetAsync<List<SportSummaryDto>>(key, ct);
        if (cached is not null) return cached;

        var sports = await _establishmentRepository.GetSportsByEstablishmentIdAsync(establishmentId, ct);
        await _cache.SetAsync(key, sports, TimeSpan.FromHours(1), ct);
        
        return sports;
    }
    
    public async Task<List<ReservationWithDetailsDto>> GetReservationsByCourtsIdAsync(IEnumerable<Guid> courtIds, EstablishmentReservationsQueryFilter filter, CancellationToken ct = default)
    {
        var reservations = await _establishmentRepository.GetReservationsByCourtsIdAsync(courtIds, filter, ct);
        return reservations;
    }

    public async Task<EstablishmentCompleteDto?> GetByIdCompleteAsync(Guid id, CancellationToken ct = default)
    {
        var key = CacheKeyByIdComplete(id);
        var cached = await _cache.GetAsync<EstablishmentCompleteDto>(key, ct);
        if (cached is not null) return cached;

        var establishment = await _establishmentRepository.GetByIdCompleteAsync(id, ct);
        if (establishment is not null)
            await _cache.SetAsync(key, establishment, TimeSpan.FromMinutes(10), ct);

        return establishment;
    }
}