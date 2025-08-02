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
}
