namespace Application.Common.Interfaces;

public interface IRealtimeNotificationService
{
    Task NotifyReservationCreatedAsync(string tenantSchema, ReservationCreatedPayload payload, CancellationToken cancellationToken = default);
}

public record ReservationCreatedPayload(
    Guid ReservationId,
    Guid CourtId,
    Guid UserId,
    DateTime StartTime,
    DateTime EndTime
);
