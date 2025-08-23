using Application.Common.QueryFilters;
using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IReservationService : IBaseService<Reservation>
{
    Task<Result<IEnumerable<DateTime>>> GetAvailableSlotsAsync(Guid courtId, DateTime day, CancellationToken cancellationToken);
    Task<Result<Guid>> ReserveAsync(Court court, Guid userId, DateTime startUtc, DateTime endUtc, CancellationToken cancellationToken);
    Task<bool> IsReservationOwnerAsync(Guid reservationId, Guid userId, CancellationToken cancellationToken);
    Task<Guid?> GetEstablishmentIdByReservationAsync(Guid reservationId, CancellationToken cancellationToken);
    Task<List<Reservation>> GetFutureReservationsByCourtAsync(Guid courtId, CancellationToken cancellationToken);
    Task<List<Reservation>> GetReservationsByCourtsIdAsync(IEnumerable<Guid> courtIds, EstablishmentReservationsQueryFilter filter, CancellationToken ct = default);
}
