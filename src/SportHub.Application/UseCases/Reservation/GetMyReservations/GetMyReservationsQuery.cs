using Application.Common.Models;
using Application.CQRS;

namespace Application.UseCases.Reservations.GetMyReservations;

public record GetMyReservationsQuery(GetMyReservationsFilter Filter) : IQuery<PagedResult<ReservationResponse>>;

public class GetMyReservationsFilter
{
    public int? Page { get; set; }
    public int? PageSize { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public record ReservationResponse(
    Guid Id,
    Guid CourtId,
    string CourtName,
    DateTime StartTimeUtc,
    DateTime EndTimeUtc,
    string? UserName,
    string? UserEmail,
    string? CreatedByName,
    string? CreatedByEmail,
    string? CreatedByRole
);
