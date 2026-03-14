using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IReservationRepository : IBaseRepository<Reservation>
{
    Task<List<Reservation>> GetByCourtAndDayAsync(Guid courtId, DateTime day);
    Task<bool> ExistsConflictAsync(Guid courtId, DateTime startUtc, DateTime endUtc);
    Task<List<Reservation>> GetByUserAsync(Guid userId);
    Task<List<Reservation>> GetByCourtAsync(Guid courtId, DateTime? date = null);
}
