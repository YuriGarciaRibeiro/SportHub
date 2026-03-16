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
