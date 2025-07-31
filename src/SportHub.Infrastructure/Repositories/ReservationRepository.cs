using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ReservationRepository : BaseRepository<Reservation>, IReservationRepository
{
    private readonly ApplicationDbContext _context;
    public ReservationRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<List<Reservation>> GetByCourtAndDayAsync(Guid courtId, DateTime day)
    {
        var dateUtc = DateTime.SpecifyKind(day.Date, DateTimeKind.Utc);

        return await _context.Reservations
            .Where(r => r.CourtId == courtId && r.StartTimeUtc.Date == dateUtc)
            .ToListAsync();
    }

    public async Task<bool> ExistsConflictAsync(Guid courtId, DateTime startUtc, DateTime endUtc)
    {
        return await _context.Reservations
            .AnyAsync(r => r.CourtId == courtId && r.StartTimeUtc < endUtc && r.EndTimeUtc > startUtc);
    }
}
