using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ReservationRepository : IReservationRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly DbSet<Reservation> _dbSet;

    public ReservationRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
        _dbSet = dbContext.Set<Reservation>();
    }

    public async Task<Reservation?> GetByIdAsync(Guid id) =>
        await _dbSet.FindAsync(id);

    public async Task<List<Reservation>> GetAllAsync() =>
        await _dbSet.ToListAsync();

    public Task AddAsync(Reservation entity)
    {
        _dbSet.Add(entity);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Reservation entity)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(Reservation entity)
    {
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<List<Reservation>> GetByIdsAsync(IEnumerable<Guid> ids) =>
        await _dbSet.Where(e => ids.Contains(e.Id)).ToListAsync();

    public async Task<bool> ExistsAsync(Guid id) =>
        await _dbSet.AnyAsync(e => e.Id == id);

    public IQueryable<Reservation> Query() =>
        _dbSet.AsQueryable();

    public Task AddManyAsync(IEnumerable<Reservation> entities)
    {
        _dbSet.AddRange(entities);
        return Task.CompletedTask;
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
