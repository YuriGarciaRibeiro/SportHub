using Application.Common.Models;
using Application.CQRS;
using Application.UseCases.Reservations.GetMyReservations;

namespace Application.UseCases.Reservations.GetAllReservations;

public record GetAllReservationsQuery(GetAllReservationsFilter Filter) : IQuery<PagedResult<ReservationResponse>>;

public class GetAllReservationsFilter
{
    public int? Page { get; set; }
    public int? PageSize { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
