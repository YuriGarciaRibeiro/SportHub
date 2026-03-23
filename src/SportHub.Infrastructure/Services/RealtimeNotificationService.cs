using Application.Common.Interfaces;
using Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.Services;

public class RealtimeNotificationService : IRealtimeNotificationService
{
    private readonly IHubContext<ReservationHub> _hubContext;

    public RealtimeNotificationService(IHubContext<ReservationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyReservationCreatedAsync(string tenantSlug, ReservationCreatedPayload payload, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients
            .Group(tenantSlug)
            .SendAsync("ReservationCreated", payload, cancellationToken);
    }
}
