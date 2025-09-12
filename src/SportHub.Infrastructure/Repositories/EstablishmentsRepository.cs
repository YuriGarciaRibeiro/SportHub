using Application.Common.Enums;
using Application.Common.QueryFilters;
using Application.UseCases.Establishments.GetEstablishments;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using EstablishmentDtos = Application.Common.Interfaces.Establishments;
using ReservationDtos = Application.Common.Interfaces.Reservations;
using SportDtos = Application.Common.Interfaces.Sports;

namespace Infrastructure.Repositories;

public class EstablishmentsRepository : BaseRepository<Establishment>, IEstablishmentsRepository
{
    private readonly ApplicationDbContext _dbContext;
    public EstablishmentsRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<EstablishmentCompleteDto?> GetByIdCompleteAsync(Guid id, double? userLatitude = null, double? userLongitude = null, CancellationToken ct = default)
    {
        var establishment = await _dbContext.Establishments
            .AsNoTracking()
            .Where(e => e.Id == id)
            .Include(e => e.Sports)
            .Include(e => e.Users)
                .ThenInclude(eu => eu.User)
            .Include(e => e.Courts)
                .ThenInclude(c => c.Sports)
            .SingleOrDefaultAsync(ct);

        if (establishment == null)
            return null;

        // Buscar avaliações do estabelecimento
        var evaluations = await _dbContext.Evaluations
            .AsNoTracking()
            .Where(e => e.TargetId == id && e.TargetType == EvaluationTargetType.Establishment)
            .Include(e => e.User)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(ct);

        if (establishment == null)
            return null;

        var distanceKm = userLatitude.HasValue && userLongitude.HasValue && 
                        establishment.Address.Latitude.HasValue && 
                        establishment.Address.Longitude.HasValue
            ? Math.Round(CalculateHaversineDistance(userLatitude.Value, userLongitude.Value, 
                establishment.Address.Latitude.Value, establishment.Address.Longitude.Value), 2)
            : (double?)null;

        return new EstablishmentCompleteDto(
            establishment.Id,
            establishment.Name,
            establishment.Description,
            establishment.PhoneNumber,
            establishment.Email,
            establishment.Website,
            establishment.OpeningTime,
            establishment.ClosingTime,
            establishment.TimeZone,
            new AddressDto(
                establishment.Address.Street,
                establishment.Address.Number,
                establishment.Address.Complement,
                establishment.Address.Neighborhood,
                establishment.Address.City,
                establishment.Address.State,
                establishment.Address.ZipCode,
                establishment.Address.Latitude,
                establishment.Address.Longitude
            ),
            establishment.ImageUrl,
            establishment.Sports.Select(s => new EstablishmentDtos.SportDto(
                s.Id,
                s.Name,
                s.Description
            )),
            establishment.Users.Select(eu => new EstablishmentUserDto(
                eu.UserId,
                $"{eu.User.FirstName} {eu.User.LastName}",
                eu.User.Email,
                eu.Role
            )),
            establishment.Courts.Select(c => new CourtDto(
                c.Id,
                c.Name,
                c.MinBookingSlots,
                c.MaxBookingSlots,
                c.SlotDurationMinutes,
                c.PricePerSlot,
                c.Sports.Select(s => new EstablishmentDtos.SportDto(
                    s.Id,
                    s.Name,
                    s.Description
                ))
            )),
            distanceKm,
            evaluations.Any() ? Math.Round(evaluations.Average(e => e.Rating), 1) : (double?)null,
            evaluations.Select(e => new EvaluationDto(
                e.Id,
                e.UserId,
                $"{e.User.FirstName} {e.User.LastName}",
                e.Rating,
                e.Comment,
                e.CreatedAt
            ))
        );
    }

    public async Task<List<EstablishmentWithDetailsDto>> GetByIdsWithDetailsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
    {
        return await _dbContext.Establishments
            .Where(e => ids.Contains(e.Id))
            .Select(e => new EstablishmentWithDetailsDto(
                e.Id,
                e.Name,
                e.Description,
                new AddressDto(
                    e.Address.Street,
                    e.Address.Number,
                    e.Address.Complement,
                    e.Address.Neighborhood,
                    e.Address.City,
                    e.Address.State,
                    e.Address.ZipCode,
                    e.Address.Latitude,
                    e.Address.Longitude
                ),
                e.ImageUrl,
                e.Sports.Select(s => new EstablishmentDtos.SportDto(
                    s.Id,
                    s.Name,
                    s.Description
                )),
                e.Users.Select(eu => new EstablishmentUserDto(
                    eu.UserId,
                    $"{eu.User.FirstName} {eu.User.LastName}",
                    eu.User.Email,
                    eu.Role
                )),
                e.Courts.Select(c => new CourtDto(
                    c.Id,
                    c.Name,
                    c.MinBookingSlots,
                    c.MaxBookingSlots,
                    c.SlotDurationMinutes,
                    c.PricePerSlot,
                    c.Sports.Select(s => new EstablishmentDtos.SportDto(
                        s.Id,
                        s.Name,
                        s.Description
                    ))
                ))
            ))
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<EstablishmentWithAddressDto?> GetByIdWithAddressAsync(Guid id, CancellationToken cancellationToken)
    {
        return _dbContext.Establishments
            .Where(e => e.Id == id)
            .Select(e => new EstablishmentWithAddressDto(
                e.Id,
                e.Name,
                e.Description,
                new AddressDto(
                    e.Address.Street,
                    e.Address.Number,
                    e.Address.Complement,
                    e.Address.Neighborhood,
                    e.Address.City,
                    e.Address.State,
                    e.Address.ZipCode,
                    e.Address.Latitude,
                    e.Address.Longitude
                ),
                e.ImageUrl
            ))
            .AsNoTracking()
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<(List<EstablishmentResponse> Items, int TotalCount)> GetFilteredAsync(GetEstablishmentsQuery query, Guid? userId, CancellationToken cancellationToken)
    {
        var dbQuery = _dbSet
            .Include(e => e.Users)
            .Include(e => e.Sports)
            .Include(e => e.Courts)
            .AsQueryable();

        if (query.ownerId.HasValue)
        {
            dbQuery = dbQuery.Where(e => e.Users.Any(u => u.UserId == query.ownerId.Value));
        }
        if (query.isAvailable.HasValue)
        {
            dbQuery = dbQuery.Where(e => e.IsDeleted == query.isAvailable.Value);
        }

        var total = await dbQuery.CountAsync(cancellationToken);

        // Aplicar ordenação no banco de dados
        var orderedQuery = ApplyOrdering(dbQuery, query);

        var establishments = await orderedQuery
            .AsNoTracking()
            .Skip((query.page - 1) * query.pageSize)
            .Take(query.pageSize)
            .ToListAsync(cancellationToken);

        var items = establishments
            .Select(e => {
                var distanceKm = query.latitude.HasValue && query.longitude.HasValue && 
                               e.Address.Latitude.HasValue && e.Address.Longitude.HasValue
                    ? Math.Round(CalculateHaversineDistance(query.latitude.Value, query.longitude.Value, 
                        e.Address.Latitude.Value, e.Address.Longitude.Value), 2)
                    : (double?)null;

                return new EstablishmentResponse(
                    e.Id,
                    e.Name,
                    e.Description,
                    new AddressResponse(
                        e.Address.Street,
                        e.Address.Number,
                        e.Address.Complement,
                        e.Address.Neighborhood,
                        e.Address.City,
                        e.Address.State,
                        e.Address.ZipCode,
                        e.Address.Latitude,
                        e.Address.Longitude
                    ),
                    e.ImageUrl,
                    e.Courts.Any() ? e.Courts.Min(c => c.PricePerSlot) : null,
                    userId.HasValue && userId.Value != Guid.Empty 
                        ? _dbContext.Favorites.Any(f => 
                            f.EntityId == e.Id && 
                            f.EntityType == FavoriteType.Establishment && 
                            f.IsDeleted == false &&
                            f.UserId == userId.Value)
                        : (bool?)null,
                    e.Sports.Select(s => new SportResponse(
                        s.Id,
                        s.Name,
                        s.Description
                    )),
                    distanceKm,
                    _dbContext.Evaluations
                        .Where(ev => ev.TargetId == e.Id && ev.TargetType == EvaluationTargetType.Establishment)
                        .Select(ev => (double)ev.Rating)
                        .DefaultIfEmpty()
                        .Average() is var avg && avg > 0 ? Math.Round(avg, 1) : (double?)null
                );
            })
            .ToList();

        // Se orderBy é "distance", ordena por distância em memória
        if (query.orderBy == EstablishmentOrderBy.Distance && query.latitude.HasValue && query.longitude.HasValue)
        {
            items = query.sortDirection == SortDirection.Desc
                ? items.OrderByDescending(e => e.DistanceKm ?? double.MaxValue).ToList()
                : items.OrderBy(e => e.DistanceKm ?? double.MaxValue).ToList();
        }
        // Se orderBy é "rating", ordena por rating médio em memória
        else if (query.orderBy == EstablishmentOrderBy.Rating)
        {
            items = query.sortDirection == SortDirection.Desc
                ? items.OrderByDescending(e => e.AverageRating ?? 0).ToList()
                : items.OrderBy(e => e.AverageRating ?? 0).ToList();
        }

        return (items, total);
    }

    public Task<List<ReservationWithDetailsDto>> GetReservationsByCourtsIdAsync(IEnumerable<Guid> courtIds, EstablishmentReservationsQueryFilter filter, CancellationToken cancellationToken)
    {
        var query = _dbContext.Reservations.AsQueryable();

        if (courtIds != null && courtIds.Any())
        {
            query = query.Where(r => courtIds.Contains(r.CourtId));
        }

        if (filter != null)
        {
            if (filter.StartTime.HasValue)
            {
                query = query.Where(r => r.StartTimeUtc >= filter.StartTime.Value);
            }

            if (filter.EndTime.HasValue)
            {
                query = query.Where(r => r.EndTimeUtc <= filter.EndTime.Value);
            }

            if (filter.UserId.HasValue)
            {
                query = query.Where(r => r.UserId == filter.UserId.Value);
            }
        }

        return query
            .Include(r => r.User)
            .Include(r => r.Court)
            .ThenInclude(c => c.Establishment)
            .Select(r => new ReservationWithDetailsDto(
                r.Id,
                r.UserId,
                r.User.FullName,
                r.User.Email,
                r.CourtId,
                r.Court.Name,
                r.StartTimeUtc,
                r.EndTimeUtc,
                r.SlotsBooked,
                r.TotalPrice,
                r.Court.Establishment.Id,
                r.Court.Establishment.Name
            ))
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<List<SportSummaryDto>> GetSportsByEstablishmentIdAsync(Guid establishmentId, CancellationToken cancellationToken)
    {
        return _context.Sports
            .Where(s => s.Establishments.Any(e => e.Id == establishmentId))
            .Select(s => new SportSummaryDto(
                s.Id,
                s.Name,
                s.Description
            ))
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<List<EstablishmentUserSummaryDto>> GetUsersByEstablishmentId(Guid establishmentId, CancellationToken cancellationToken)
    {
        return await _context.EstablishmentUsers
            .Where(eu => eu.EstablishmentId == establishmentId)
            .Select(eu => new EstablishmentUserSummaryDto(
                eu.UserId,
                eu.User.FirstName,
                eu.User.LastName,
                eu.User.Email,
                eu.User.Email,
                eu.Role
            ))
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    private static IQueryable<Establishment> ApplyOrdering(IQueryable<Establishment> query, GetEstablishmentsQuery request)
    {
        return request.orderBy switch
        {
            EstablishmentOrderBy.Name => request.sortDirection == SortDirection.Desc
                ? query.OrderByDescending(e => e.Name)
                : query.OrderBy(e => e.Name),
                
            EstablishmentOrderBy.Newest => request.sortDirection == SortDirection.Asc
                ? query.OrderBy(e => e.CreatedAt)
                : query.OrderByDescending(e => e.CreatedAt),
                
            EstablishmentOrderBy.Rating =>
                query.OrderBy(e => e.Name),
                
            EstablishmentOrderBy.Popularity => request.sortDirection == SortDirection.Desc
                ? query.OrderByDescending(e => e.Courts.Count()) // TODO: Implementar popularity real  
                : query.OrderBy(e => e.Courts.Count()),
                
            EstablishmentOrderBy.Distance when request.latitude.HasValue && request.longitude.HasValue =>
                query.OrderBy(e => e.Name),
                
            _ => query.OrderBy(e => e.Name)
        };
    }

    private static double CalculateHaversineDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double earthRadiusKm = 6371.0;
        
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        
        return earthRadiusKm * c;
    }

    private static double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }
}