using Application.Common.Interfaces;
using Application.Common.QueryFilters;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ReservationRepository : BaseRepository<Reservation>, IReservationRepository
{
    private readonly ApplicationDbContext _dbContext;
    public ReservationRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Reservation>> GetByCourtAndDayAsync(Guid courtId, DateTime day, CancellationToken cancellationToken)
    {
        var dateUtc = DateTime.SpecifyKind(day.Date, DateTimeKind.Utc);

        return await _dbContext.Reservations
            .Where(r => r.CourtId == courtId && r.StartTimeUtc.Date == dateUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsConflictAsync(Guid courtId, DateTime startUtc, DateTime endUtc, CancellationToken cancellationToken)
    {
        return await _dbContext.Reservations
            .AnyAsync(r => r.CourtId == courtId && r.StartTimeUtc < endUtc && r.EndTimeUtc > startUtc, cancellationToken);
    }

    public async Task<List<Reservation>> GetFutureReservationsByCourtAsync(Guid courtId, CancellationToken cancellationToken)
    {
        return await _dbContext.Reservations
            .Where(r => r.CourtId == courtId && r.StartTimeUtc > DateTime.UtcNow)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsReservationOwnerAsync(Guid reservationId, Guid userId, CancellationToken cancellationToken)
    {
        return await _dbContext.Reservations
            .AnyAsync(r => r.Id == reservationId && r.UserId == userId, cancellationToken);
    }

    public async Task<Guid?> GetEstablishmentIdByReservationAsync(Guid reservationId, CancellationToken cancellationToken)
    {
        return await _dbContext.Reservations
            .Where(r => r.Id == reservationId)
            .Select(r => r.Court.EstablishmentId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<List<Reservation>> GetReservationsByCourtsIdAsync(IEnumerable<Guid> courtIds, EstablishmentReservationsQueryFilter filter, CancellationToken ct = default)
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

        return query.ToListAsync(ct);
    }
}
