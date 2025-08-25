using Application.Common.QueryFilters;
using Application.UseCases.Establishments.GetEstablishments;
using Domain.Entities;
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

    public Task<EstablishmentDtos.EstablishmentCompleteDto?> GetByIdCompleteAsync(Guid id, CancellationToken ct = default)
    {
        return _dbContext.Establishments
            .AsNoTracking()
            .Where(e => e.Id == id)
            .Select(e => new EstablishmentDtos.EstablishmentCompleteDto(
                e.Id,
                e.Name,
                e.Description,
                new EstablishmentDtos.AddressDto(
                    e.Address.Street,
                    e.Address.Number,
                    e.Address.Complement,
                    e.Address.Neighborhood,
                    e.Address.City,
                    e.Address.State,
                    e.Address.ZipCode
                ),
                e.ImageUrl,
                e.Sports.Select(s => new EstablishmentDtos.SportDto(
                    s.Id,
                    s.Name,
                    s.Description
                )),
                e.Users.Select(eu => new EstablishmentDtos.EstablishmentUserDto(
                    eu.UserId,
                    $"{eu.User.FirstName} {eu.User.LastName}",
                    eu.User.Email,
                    eu.Role
                )),
                e.Courts.Select(c => new EstablishmentDtos.CourtDto(
                    c.Id,
                    c.Name,
                    c.MinBookingSlots,
                    c.MaxBookingSlots,
                    c.SlotDurationMinutes,
                    c.TimeZone,
                    c.Sports.Select(s => new EstablishmentDtos.SportDto(
                        s.Id,
                        s.Name,
                        s.Description
                    ))
                ))
            ))
            .SingleOrDefaultAsync(ct);
    }

    public async Task<List<EstablishmentDtos.EstablishmentWithDetailsDto>> GetByIdsWithDetailsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
    {
        return await _dbContext.Establishments
            .Where(e => ids.Contains(e.Id))
            .Select(e => new EstablishmentDtos.EstablishmentWithDetailsDto(
                e.Id,
                e.Name,
                e.Description,
                new EstablishmentDtos.AddressDto(
                    e.Address.Street,
                    e.Address.Number,
                    e.Address.Complement,
                    e.Address.Neighborhood,
                    e.Address.City,
                    e.Address.State,
                    e.Address.ZipCode
                ),
                e.ImageUrl,
                e.Sports.Select(s => new EstablishmentDtos.SportDto(
                    s.Id,
                    s.Name,
                    s.Description
                )),
                e.Users.Select(eu => new EstablishmentDtos.EstablishmentUserDto(
                    eu.UserId,
                    $"{eu.User.FirstName} {eu.User.LastName}",
                    eu.User.Email,
                    eu.Role
                )),
                e.Courts.Select(c => new EstablishmentDtos.CourtDto(
                    c.Id,
                    c.Name,
                    c.MinBookingSlots,
                    c.MaxBookingSlots,
                    c.SlotDurationMinutes,
                    c.TimeZone,
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

    public Task<EstablishmentDtos.EstablishmentWithAddressDto?> GetByIdWithAddressAsync(Guid id, CancellationToken cancellationToken)
    {
        return _dbContext.Establishments
            .Where(e => e.Id == id)
            .Select(e => new EstablishmentDtos.EstablishmentWithAddressDto(
                e.Id,
                e.Name,
                e.Description,
                new EstablishmentDtos.AddressDto(
                    e.Address.Street,
                    e.Address.Number,
                    e.Address.Complement,
                    e.Address.Neighborhood,
                    e.Address.City,
                    e.Address.State,
                    e.Address.ZipCode
                ),
                e.ImageUrl
            ))
            .AsNoTracking()
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<(List<EstablishmentResponse> Items, int TotalCount)> GetFilteredAsync(GetEstablishmentsQuery query, CancellationToken cancellationToken)
    {
        var dbQuery = _dbSet
            .Include(e => e.Users)
            .Include(e => e.Sports)
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

        var items = await dbQuery
            .AsNoTracking()
            .OrderBy(e => e.Name)
            .Skip((query.page - 1) * query.pageSize)
            .Take(query.pageSize)
            .Select(e => new EstablishmentResponse(
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
                    e.Address.ZipCode
                ),
                e.ImageUrl,
                e.Sports.Select(s => new SportResponse(
                    s.Id,
                    s.Name,
                    s.Description
                ))
            ))
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public Task<List<ReservationDtos.ReservationWithDetailsDto>> GetReservationsByCourtsIdAsync(IEnumerable<Guid> courtIds, EstablishmentReservationsQueryFilter filter, CancellationToken cancellationToken)
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
            .Select(r => new ReservationWithDetailsDto(
                r.Id,
                r.UserId,
                r.User.FullName,
                r.User.Email,
                r.CourtId,
                r.Court.Name,
                r.StartTimeUtc,
                r.EndTimeUtc
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
}

public record EstablishmentCompleteResponse(
    Guid Id,
    string Name,
    string Description,
    AddressResponse Address,
    string? ImageUrl,
    IEnumerable<SportResponse> Sports,
    IEnumerable<UserCompleteResponse> Users,
    IEnumerable<CourtCompleteResponse> Courts
);

public record AddressCompleteResponse(
    string Street,
    string Number,
    string? Complement,
    string Neighborhood,
    string City,
    string State,
    string ZipCode
);

public record SportCompleteResponse(
    Guid Id,
    string Name,
    string Description
);

public record UserCompleteResponse(
    Guid Id,
    string Name,
    string Email
);

public record CourtCompleteResponse(
    Guid Id,
    string Name,
    string Location
);
