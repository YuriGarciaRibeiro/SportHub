using Application.CQRS;

namespace Application.UseCases.Reservations.GetActiveReservation;

public record GetActiveReservationQuery : IQuery<ActiveReservationResponse?>;

public record ActiveReservationResponse(
    Guid Id,
    string CourtName,
    DateTime StartTimeUtc,
    DateTime EndTimeUtc
);
