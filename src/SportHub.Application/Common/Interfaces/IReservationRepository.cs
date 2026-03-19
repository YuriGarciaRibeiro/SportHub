using Application.Common.Models;
using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IReservationRepository
{
    Task<Reservation?> GetByIdAsync(Guid id);
    Task<List<Reservation>> GetAllAsync();
    Task AddAsync(Reservation entity);
    Task UpdateAsync(Reservation entity);
    Task RemoveAsync(Reservation entity);
    Task<List<Reservation>> GetByIdsAsync(IEnumerable<Guid> ids);
    Task<bool> ExistsAsync(Guid id);
    IQueryable<Reservation> Query();
    Task AddManyAsync(IEnumerable<Reservation> entities);
    Task<List<Reservation>> GetByCourtAndDayAsync(Guid courtId, DateTime day);
    Task<bool> ExistsConflictAsync(Guid courtId, DateTime startUtc, DateTime endUtc);
    Task<List<Reservation>> GetByUserAsync(Guid userId);
    Task<List<Reservation>> GetByCourtAsync(Guid courtId, DateTime? date = null);
    Task<PagedResult<Reservation>> GetPagedAsync(
        int page,
        int pageSize,
        Guid? courtId = null,
        Guid? userId = null,
        DateTime? startDate = null,
        DateTime? endDate = null);
    Task<List<CustomerReservationMetrics>> GetMetricsByUserIdsAsync(IEnumerable<Guid> userIds, CancellationToken ct = default);
    Task<CustomerReservationMetrics?> GetMetricsByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<List<CourtFrequency>> GetTopCourtsByUserAsync(Guid userId, int top, CancellationToken ct = default);
    Task<int> CountByDayAsync(DateTime day);
    Task<decimal> GetTotalRevenueByDayAsync(DateTime day);
    Task<List<DailyRevenue>> GetDailyRevenueAsync(DateTime fromUtc, DateTime toUtc, CancellationToken ct = default);
    Task<List<CourtOccupancy>> GetCourtOccupancyTodayAsync(DateTime todayStartUtc, CancellationToken ct = default);
    Task<int> CountByPeriodAsync(DateTime fromUtc, DateTime toUtc, CancellationToken ct = default);
    Task<List<CourtRevenue>> GetRevenueByCourtAsync(DateTime fromUtc, DateTime toUtc, CancellationToken ct = default);
    Task<CancellationStats> GetCancellationStatsAsync(DateTime fromUtc, DateTime toUtc, CancellationToken ct = default);
    Task<List<TopCustomer>> GetTopCustomersAsync(DateTime fromUtc, DateTime toUtc, int top, CancellationToken ct = default);
}

public class CustomerReservationMetrics
{
    public Guid UserId { get; set; }
    public int TotalReservations { get; set; }
    public decimal TotalSpent { get; set; }
    public DateTime? LastReservationAt { get; set; }
}

public class CourtFrequency
{
    public Guid CourtId { get; set; }
    public string CourtName { get; set; } = null!;
    public int Count { get; set; }
}

public class DailyRevenue
{
    public DateOnly Date { get; set; }
    public decimal Revenue { get; set; }
}

public class CourtOccupancy
{
    public Guid CourtId { get; set; }
    public string CourtName { get; set; } = null!;
    public int TotalSlots { get; set; }
    public int BookedSlots { get; set; }
}

public class CourtRevenue
{
    public Guid CourtId { get; set; }
    public string CourtName { get; set; } = null!;
    public decimal Revenue { get; set; }
    public int Reservations { get; set; }
}

public class CancellationStats
{
    public int Count { get; set; }
    public decimal Revenue { get; set; }
}

public class TopCustomer
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public decimal TotalSpent { get; set; }
    public int Reservations { get; set; }
}
