using Application.Common.QueryFilters;
using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IReservationRepository : IBaseRepository<Reservation>
{
    Task<List<Reservation>> GetByCourtAndDayAsync(Guid courtId, DateTime day, CancellationToken cancellationToken);
    Task<List<Reservation>> GetFutureReservationsByCourtAsync(Guid courtId, CancellationToken cancellationToken);
    Task<bool> ExistsConflictAsync(Guid courtId, DateTime startUtc, DateTime endUtc, CancellationToken cancellationToken);
    Task<bool> IsReservationOwnerAsync(Guid reservationId, Guid userId, CancellationToken cancellationToken);
    Task<Guid?> GetEstablishmentIdByReservationAsync(Guid reservationId, CancellationToken cancellationToken);
    Task<List<Reservation>> GetReservationsByCourtsIdAsync(IEnumerable<Guid> courtIds, EstablishmentReservationsQueryFilter filter, CancellationToken ct = default);
}
