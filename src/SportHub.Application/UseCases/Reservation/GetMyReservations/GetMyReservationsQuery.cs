using Application.CQRS;

namespace Application.UseCases.Reservations.GetMyReservations;

public record GetMyReservationsQuery : IQuery<List<ReservationResponse>>;

public record ReservationResponse(
    Guid Id,
    Guid CourtId,
    string CourtName,
    DateTime StartTimeUtc,
    DateTime EndTimeUtc
);
