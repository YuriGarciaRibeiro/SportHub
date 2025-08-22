using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IReservationRepository : IBaseRepository<Reservation>
{
    Task<List<Reservation>> GetByCourtAndDayAsync(Guid courtId, DateTime day, CancellationToken cancellationToken);
    Task<List<Reservation>> GetFutureReservationsByCourtAsync(Guid courtId, CancellationToken cancellationToken);
    Task<bool> ExistsConflictAsync(Guid courtId, DateTime startUtc, DateTime endUtc, CancellationToken cancellationToken);
}
