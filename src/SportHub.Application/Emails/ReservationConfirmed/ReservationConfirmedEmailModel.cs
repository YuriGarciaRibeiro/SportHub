namespace Application.Emails.reservation_confirmed;

public sealed record ReservationConfirmedEmailModel(
    string establishmentName,
    string startDateTime,
    string endDateTime,
    string userName,
    string reservationId,
    string courtName,
    string duration,
    string price,
    string supportUrl,
    string year
);