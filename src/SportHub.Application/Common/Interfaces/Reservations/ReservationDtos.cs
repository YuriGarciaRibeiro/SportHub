namespace Application.Common.Interfaces.Reservations;

public record ReservationDayDto(
    Guid Id,
    Guid UserId,
    Guid CourtId,
    DateTime StartTimeUtc,
    DateTime EndTimeUtc
);

public record FutureReservationDto(
    Guid Id,
    Guid UserId,
    Guid CourtId,
    string CourtName,
    DateTime StartTimeUtc,
    DateTime EndTimeUtc
);

public record ReservationSummaryDto(
    Guid Id,
    Guid UserId,
    Guid CourtId,
    DateTime StartTimeUtc,
    DateTime EndTimeUtc
);

public record ReservationWithDetailsDto(
    Guid Id,
    Guid UserId,
    string UserName,
    string UserEmail,
    Guid CourtId,
    string CourtName,
    DateTime StartTimeUtc,
    DateTime EndTimeUtc
);

public record ReservationCompleteDto(
    Guid Id,
    Guid UserId,
    string UserName,
    string UserEmail,
    Guid CourtId,
    string CourtName,
    Guid EstablishmentId,
    string EstablishmentName,
    DateTime StartTimeUtc,
    DateTime EndTimeUtc
);
