using Application.Common.Models;
using Application.CQRS;
using Application.UseCases.Reservations.GetMyReservations;

namespace Application.UseCases.Reservations.GetCourtReservations;

public record GetCourtReservationsQuery(Guid CourtId, GetCourtReservationsFilter Filter) : IQuery<PagedResult<ReservationResponse>>;

public class GetCourtReservationsFilter
{
    public int? Page { get; set; }
    public int? PageSize { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
