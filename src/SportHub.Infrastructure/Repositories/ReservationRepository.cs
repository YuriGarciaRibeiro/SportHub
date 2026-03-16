using Application.Common.Interfaces;
using Application.Common.Models;
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

    public async Task<PagedResult<Reservation>> GetPagedAsync(
        int page,
        int pageSize,
        Guid? courtId = null,
        Guid? userId = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var query = _dbContext.Reservations
            .Include(r => r.Court)
            .Include(r => r.User)
            .AsQueryable();

        if (courtId.HasValue)
            query = query.Where(r => r.CourtId == courtId.Value);

        if (userId.HasValue)
            query = query.Where(r => r.UserId == userId.Value);

        if (startDate.HasValue)
        {
            var startUtc = DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc);
            query = query.Where(r => r.StartTimeUtc >= startUtc);
        }

        if (endDate.HasValue)
        {
            var endUtc = DateTime.SpecifyKind(endDate.Value.Date.AddDays(1), DateTimeKind.Utc);
            query = query.Where(r => r.StartTimeUtc < endUtc);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(r => r.StartTimeUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Reservation>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<List<CustomerReservationMetrics>> GetMetricsByUserIdsAsync(IEnumerable<Guid> userIds, CancellationToken ct = default)
    {
        var ids = userIds.ToList();
        return await _dbContext.Reservations
            .Where(r => ids.Contains(r.UserId))
            .GroupBy(r => r.UserId)
            .Select(g => new CustomerReservationMetrics
            {
                UserId = g.Key,
                TotalReservations = g.Count(),
                TotalSpent = g.Sum(r => (decimal)(r.EndTimeUtc - r.StartTimeUtc).TotalMinutes / 60m * r.Court.PricePerHour),
                LastReservationAt = g.Max(r => (DateTime?)r.StartTimeUtc)
            })
            .ToListAsync(ct);
    }

    public async Task<CustomerReservationMetrics?> GetMetricsByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _dbContext.Reservations
            .Where(r => r.UserId == userId)
            .GroupBy(r => r.UserId)
            .Select(g => new CustomerReservationMetrics
            {
                UserId = g.Key,
                TotalReservations = g.Count(),
                TotalSpent = g.Sum(r => (decimal)(r.EndTimeUtc - r.StartTimeUtc).TotalMinutes / 60m * r.Court.PricePerHour),
                LastReservationAt = g.Max(r => (DateTime?)r.StartTimeUtc)
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<CourtFrequency>> GetTopCourtsByUserAsync(Guid userId, int top, CancellationToken ct = default)
    {
        return await _dbContext.Reservations
            .Where(r => r.UserId == userId)
            .GroupBy(r => new { r.CourtId, r.Court.Name })
            .Select(g => new CourtFrequency
            {
                CourtId = g.Key.CourtId,
                CourtName = g.Key.Name,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .Take(top)
            .ToListAsync(ct);
    }
}
