using Application.Common.Interfaces.Base;
using Application.Common.QueryFilters;
using Domain.Entities;

namespace Application.Common.Interfaces.Reservations;

public interface IReservationRepository : IBaseRepository<Reservation>
{
    Task<List<ReservationDayDto>> GetByCourtAndDayAsync(Guid courtId, DateTime day, CancellationToken cancellationToken);
    Task<List<FutureReservationDto>> GetFutureReservationsByCourtAsync(Guid courtId, CancellationToken cancellationToken);
    Task<List<ReservationSummaryDto>> GetReservationsByCourtsIdAsync(IEnumerable<Guid> courtIds, EstablishmentReservationsQueryFilter filter, CancellationToken ct = default);
    Task<(IEnumerable<ReservationWithDetailsDto> Items, int Total)> GetReservationsByUserIdAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken);

    Task<bool> ExistsConflictAsync(Guid courtId, DateTime startUtc, DateTime endUtc, CancellationToken cancellationToken);
    Task<bool> IsReservationOwnerAsync(Guid reservationId, Guid userId, CancellationToken cancellationToken);
    Task<Guid?> GetEstablishmentIdByReservationAsync(Guid reservationId, CancellationToken cancellationToken);
}
