using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SportHub.Infrastructure.Extensions;

namespace Infrastructure.Repositories;

public class ReservationRepository : IReservationRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly DbSet<Reservation> _dbSet;
    private readonly ITenantContext _tenantContext;

    public ReservationRepository(ApplicationDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _dbSet = dbContext.Set<Reservation>();
        _tenantContext = tenantContext;
    }

    public async Task<Reservation?> GetByIdAsync(Guid id, GetReservationSettings? includeSettings = null)
    {
         return await _dbContext.Reservations
        .If(includeSettings?.AsNoTracking == true, q => q.AsNoTracking())
        .If(includeSettings?.IncludeTenant == true, q => q.Include(r => r.Tenant))
        .If(includeSettings?.IncludeCourt == true, q => q.Include(r => r.Court))
        .If(includeSettings?.IncludeUser == true, q => q.Include(r => r.User))
        .AsSplitQuery()
        .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<List<Reservation>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

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

    public async Task<List<Reservation>> GetByIdsAsync(IEnumerable<Guid> ids)
    {
        return await _dbSet.Where(e => ids.Contains(e.Id)).ToListAsync();
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _dbSet.AnyAsync(e => e.Id == id);
    }

    public IQueryable<Reservation> Query()
    {
        return _dbSet.AsQueryable();
    }

    public Task AddManyAsync(IEnumerable<Reservation> entities)
    {
        _dbSet.AddRange(entities);
        return Task.CompletedTask;
    }

    public async Task<List<Reservation>> GetByCourtAndDayAsync(Guid courtId, DateTime day)
    {
        var dateUtc = DateTime.SpecifyKind(day.Date, DateTimeKind.Utc);
        return await _dbContext.Reservations
            .Where(r => r.CourtId == courtId && r.StartTimeUtc.Date == dateUtc && r.Status != ReservationStatus.Cancelled)
            .ToListAsync();
    }

    public async Task<List<Reservation>> GetByCourtIdsAndPeriodAsync(
        IEnumerable<Guid> courtIds,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken ct = default)
    {
        var ids = courtIds.ToList();
        return await _dbContext.Reservations
            .IgnoreQueryFilters()
            .Where(r => ids.Contains(r.CourtId) && !r.IsDeleted && r.StartTimeUtc >= fromUtc && r.StartTimeUtc < toUtc)
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsConflictAsync(Guid courtId, DateTime startUtc, DateTime endUtc)
    {
        return await _dbContext.Reservations
            .AnyAsync(r => r.CourtId == courtId && r.StartTimeUtc < endUtc && r.EndTimeUtc > startUtc && r.Status != ReservationStatus.Cancelled);
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
            .Include(r => r.CreatedByUser)
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

    public async Task<decimal> GetTotalSpentByUserAsync(Guid userId, CancellationToken ct = default)
    {
        return await _dbContext.Reservations
            .Where(r => r.UserId == userId &&
                        (r.Status == ReservationStatus.Confirmed || r.Status == ReservationStatus.Completed))
            .SumAsync(r => r.TotalPrice, ct);
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

    public async Task<int> CountByDayAsync(DateTime day)
    {
        var startUtc = DateTime.SpecifyKind(day.Date, DateTimeKind.Utc);
        var endUtc = startUtc.AddDays(1);
        return await _dbContext.Reservations
            .Where(r => r.StartTimeUtc >= startUtc && r.StartTimeUtc < endUtc)
            .CountAsync();
    }

    public async Task<decimal> GetTotalRevenueByDayAsync(DateTime day)
    {
        var startUtc = DateTime.SpecifyKind(day.Date, DateTimeKind.Utc);
        var endUtc = startUtc.AddDays(1);
        var reservations = await _dbContext.Reservations
            .Include(r => r.Court)
            .Where(r => r.StartTimeUtc >= startUtc && r.StartTimeUtc < endUtc)
            .ToListAsync();

        return reservations.Sum(r =>
            (decimal)(r.EndTimeUtc - r.StartTimeUtc).TotalMinutes / 60m * r.Court.PricePerHour);
    }

    public async Task<List<DailyRevenue>> GetDailyRevenueAsync(DateTime fromUtc, DateTime toUtc, CancellationToken ct = default)
    {
        var reservations = await _dbContext.Reservations
            .Include(r => r.Court)
            .Where(r => r.StartTimeUtc >= fromUtc && r.StartTimeUtc < toUtc)
            .Select(r => new
            {
                Date = r.StartTimeUtc.Date,
                Revenue = (decimal)(r.EndTimeUtc - r.StartTimeUtc).TotalMinutes / 60m * r.Court.PricePerHour
            })
            .ToListAsync(ct);

        return reservations
            .GroupBy(r => r.Date)
            .Select(g => new DailyRevenue
            {
                Date = DateOnly.FromDateTime(g.Key),
                Revenue = g.Sum(r => r.Revenue)
            })
            .OrderBy(d => d.Date)
            .ToList();
    }

    public async Task<int> CountByPeriodAsync(DateTime fromUtc, DateTime toUtc, CancellationToken ct = default)
    {
        return await _dbContext.Reservations
            .Where(r => r.StartTimeUtc >= fromUtc && r.StartTimeUtc < toUtc)
            .CountAsync(ct);
    }

    public async Task<List<CourtRevenue>> GetRevenueByCourtAsync(DateTime fromUtc, DateTime toUtc, CancellationToken ct = default)
    {
        var reservations = await _dbContext.Reservations
            .Include(r => r.Court)
            .Where(r => r.StartTimeUtc >= fromUtc && r.StartTimeUtc < toUtc)
            .Select(r => new
            {
                r.CourtId,
                r.Court.Name,
                Revenue = (decimal)(r.EndTimeUtc - r.StartTimeUtc).TotalMinutes / 60m * r.Court.PricePerHour
            })
            .ToListAsync(ct);

        return reservations
            .GroupBy(r => new { r.CourtId, r.Name })
            .Select(g => new CourtRevenue
            {
                CourtId = g.Key.CourtId,
                CourtName = g.Key.Name,
                Revenue = g.Sum(r => r.Revenue),
                Reservations = g.Count()
            })
            .OrderByDescending(c => c.Revenue)
            .ToList();
    }

    public async Task<CancellationStats> GetCancellationStatsAsync(DateTime fromUtc, DateTime toUtc, CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId;
        var cancelled = await _dbContext.Reservations
            .Include(r => r.Court)
            .Where(r => r.TenantId == tenantId && r.Status == ReservationStatus.Cancelled && r.UpdatedAt >= fromUtc && r.UpdatedAt < toUtc)
            .Select(r => new
            {
                Revenue = (decimal)(r.EndTimeUtc - r.StartTimeUtc).TotalMinutes / 60m * r.Court.PricePerHour
            })
            .ToListAsync(ct);

        return new CancellationStats
        {
            Count = cancelled.Count,
            Revenue = cancelled.Sum(r => r.Revenue)
        };
    }

    public async Task<List<TopCustomer>> GetTopCustomersAsync(DateTime fromUtc, DateTime toUtc, int top, CancellationToken ct = default)
    {
        var reservations = await _dbContext.Reservations
            .Include(r => r.Court)
            .Include(r => r.User)
            .Where(r => r.StartTimeUtc >= fromUtc && r.StartTimeUtc < toUtc)
            .Select(r => new
            {
                r.UserId,
                r.User.FirstName,
                r.User.LastName,
                UserEmail = r.User.Email,
                Revenue = (decimal)(r.EndTimeUtc - r.StartTimeUtc).TotalMinutes / 60m * r.Court.PricePerHour
            })
            .ToListAsync(ct);

        return reservations
            .GroupBy(r => new { r.UserId, r.FirstName, r.LastName, r.UserEmail })
            .Select(g => new TopCustomer
            {
                UserId = g.Key.UserId,
                Name = $"{g.Key.FirstName} {g.Key.LastName}".Trim(),
                Email = g.Key.UserEmail ?? "—",
                TotalSpent = g.Sum(r => r.Revenue),
                Reservations = g.Count()
            })
            .OrderByDescending(c => c.TotalSpent)
            .Take(top)
            .ToList();
    }

    public async Task<List<CourtOccupancy>> GetCourtOccupancyTodayAsync(DateTime todayStartUtc, CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId;
        var todayEndUtc = todayStartUtc.AddDays(1);

        var courts = await _dbContext.Courts
            .IgnoreQueryFilters()
            .Where(c => c.TenantId == tenantId && !c.IsDeleted)
            .ToListAsync(ct);

        var bookedSlotsByCourt = await _dbContext.Reservations
            .Where(r => r.StartTimeUtc >= todayStartUtc && r.StartTimeUtc < todayEndUtc)
            .GroupBy(r => r.CourtId)
            .Select(g => new { CourtId = g.Key, BookedSlots = g.Count() })
            .ToListAsync(ct);

        var bookedMap = bookedSlotsByCourt.ToDictionary(x => x.CourtId, x => x.BookedSlots);

        return courts.Select(c =>
        {
            var openingMinutes = c.OpeningTime.ToTimeSpan().TotalMinutes;
            var closingMinutes = c.ClosingTime.ToTimeSpan().TotalMinutes;
            var totalMinutes = closingMinutes - openingMinutes;
            var totalSlots = (int)(totalMinutes / c.SlotDurationMinutes);

            return new CourtOccupancy
            {
                CourtId = c.Id,
                CourtName = c.Name,
                TotalSlots = totalSlots > 0 ? totalSlots : 1,
                BookedSlots = bookedMap.TryGetValue(c.Id, out var booked) ? booked : 0
            };
        }).ToList();
    }
}
