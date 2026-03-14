using Application.Common.Interfaces;
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

    public async Task<List<Reservation>> GetByCourtAndDayAsync(Guid courtId, DateTime day)
    {
        var dateUtc = DateTime.SpecifyKind(day.Date, DateTimeKind.Utc);

        return await _dbContext.Reservations
            .Where(r => r.CourtId == courtId && r.StartTimeUtc.Date == dateUtc)
            .ToListAsync();
    }

    public async Task<bool> ExistsConflictAsync(Guid courtId, DateTime startUtc, DateTime endUtc)
    {
        return await _dbContext.Reservations
            .AnyAsync(r => r.CourtId == courtId && r.StartTimeUtc < endUtc && r.EndTimeUtc > startUtc);
    }

    public async Task<List<Reservation>> GetByUserAsync(Guid userId)
    {
        return await _dbContext.Reservations
            .Include(r => r.Court)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.StartTimeUtc)
            .ToListAsync();
    }

    public async Task<List<Reservation>> GetByCourtAsync(Guid courtId, DateTime? date = null)
    {
        var query = _dbContext.Reservations
            .Include(r => r.Court)
            .Where(r => r.CourtId == courtId);

        if (date.HasValue)
        {
            var dateUtc = DateTime.SpecifyKind(date.Value.Date, DateTimeKind.Utc);
            query = query.Where(r => r.StartTimeUtc.Date == dateUtc);
        }

        return await query
            .OrderByDescending(r => r.StartTimeUtc)
            .ToListAsync();
    }
}
