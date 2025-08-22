using Application.Common.Interfaces;
using Application.Common.QueryFilters;
using Application.UseCases.Establishments.GetEstablishments;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class EstablishmentsRepository : BaseRepository<Establishment>, IEstablishmentsRepository
{
    private readonly ApplicationDbContext _dbContext;
    public EstablishmentsRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Establishment>> GetByIdsWithDetailsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
    {
        return await _dbContext.Establishments
            .Where(e => ids.Contains(e.Id))
            .Include(e => e.Courts)
                .ThenInclude(c => c.Sports)
            .Include(e => e.Users)
            .Include(e => e.Sports)
            .AsSplitQuery()
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<Establishment?> GetByIdWithAddressAsync(Guid id, CancellationToken cancellationToken)
    {
        return _dbContext.Establishments
            .Where(e => e.Id == id)
            .Include(e => e.Address)
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

    public Task<List<Reservation>> GetReservationsByCourtsIdAsync(IEnumerable<Guid> courtIds, EstablishmentReservationsQueryFilter filter, CancellationToken cancellationToken)
    {
        var query = _context.Reservations
            .Where(r => courtIds.Contains(r.CourtId));

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

        return query.ToListAsync(cancellationToken);
    }

    public Task<List<Sport>> GetSportsByEstablishmentIdAsync(Guid establishmentId, CancellationToken cancellationToken)
    {
        return _context.Sports
            .Where(s => s.Establishments.Any(e => e.Id == establishmentId))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<User>> GetUsersByEstablishmentId(Guid establishmentId, CancellationToken cancellationToken)
    {
        return await _context.EstablishmentUsers
            .Where(eu => eu.EstablishmentId == establishmentId)
            .Include(eu => eu.User)
            .Select(eu => eu.User)
            .ToListAsync(cancellationToken);
    }
}
