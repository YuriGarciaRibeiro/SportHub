using Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class PeakHoursCalculator(
    ICourtsRepository courtsRepository,
    ITenantRepository tenantRepository,
    IReservationRepository reservationRepository,
    ILogger<PeakHoursCalculator> logger) : IPeakHoursCalculator
{
    private const int MinReservationsRequired = 10;

    public async Task CalculateAndSetPeakHoursAsync(CancellationToken ct = default)
    {
        logger.LogInformation("[PeakHours] Orquestrador iniciado.");

        var tenantIds = await tenantRepository.GetAllPeakHoursEnabledAsync(ct);

        if (!tenantIds.Any())
        {
            logger.LogInformation("[PeakHours] Nenhum tenant com peak hours habilitado. Encerrando.");
            return;
        }

        logger.LogInformation("[PeakHours] {Count} tenant(s) encontrado(s). Processando sequencialmente.", tenantIds.Count);

        foreach (var tenantId in tenantIds)
            await ProcessTenantAsync(tenantId, ct);
    }

    public async Task CalculateAndSetPeakHoursAsync(Guid tenantId, CancellationToken ct = default)
    {
        logger.LogInformation("[PeakHours] Processando tenant {TenantId}.", tenantId);
        await ProcessTenantAsync(tenantId, ct);
    }

    private async Task ProcessTenantAsync(Guid tenantId, CancellationToken ct)
    {
        var courts = await courtsRepository.GetByTenantIdsAsync([tenantId]);

        if (courts.Count == 0)
        {
            logger.LogInformation("[PeakHours] Tenant {TenantId}: nenhuma quadra encontrada.", tenantId);
            return;
        }

        logger.LogInformation("[PeakHours] Tenant {TenantId}: {Count} quadra(s) encontrada(s).", tenantId, courts.Count);

        var now = DateTime.UtcNow;
        var fromUtc = now.AddDays(-7);
        var courtIds = courts.Select(c => c.Id);

        var reservations = await reservationRepository.GetByCourtIdsAndPeriodAsync(courtIds, fromUtc, now, ct);

        logger.LogInformation("[PeakHours] Tenant {TenantId}: {Count} reserva(s) nos últimos 7 dias.", tenantId, reservations.Count);

        var reservationsByCourt = reservations.GroupBy(r => r.CourtId).ToDictionary(g => g.Key, g => g.ToList());

        var updatedCount = 0;
        foreach (var court in courts)
        {
            if (!reservationsByCourt.TryGetValue(court.Id, out var courtReservations))
            {
                logger.LogDebug("[PeakHours] Quadra '{Name}' sem reservas no período. Ignorando.", court.Name);
                continue;
            }

            if (courtReservations.Count < MinReservationsRequired)
            {
                logger.LogDebug("[PeakHours] Quadra '{Name}' com apenas {Count} reserva(s) (mínimo: {Min}). Ignorando.",
                    court.Name, courtReservations.Count, MinReservationsRequired);
                continue;
            }

            var peak = FindPeakHour(court, courtReservations);
            if (peak is null)
            {
                logger.LogDebug("[PeakHours] Quadra '{Name}' sem horário de pico identificável.", court.Name);
                continue;
            }

            court.PeakStartTime = peak.Value.AddHours(-1);
            court.PeakEndTime = peak.Value.AddHours(1);
            updatedCount++;

            logger.LogInformation("[PeakHours] Quadra '{Name}': pico calculado {Start}-{End}.",
                court.Name, court.PeakStartTime, court.PeakEndTime);
        }

        await courtsRepository.UpdateManyAsync(courts);

        logger.LogInformation("[PeakHours] Tenant {TenantId}: finalizado. {Updated}/{Total} quadra(s) atualizada(s).",
            tenantId, updatedCount, courts.Count);
    }

    private static TimeOnly? FindPeakHour(Domain.Entities.Court court, List<Domain.Entities.Reservation> reservations)
    {
        // Count how many reservations overlap each hour slot within the court's opening hours
        var slotCounts = new Dictionary<int, int>();

        var openHour = court.OpeningTime.Hour;
        var closeHour = court.ClosingTime.Hour;

        for (var h = openHour; h < closeHour; h++)
            slotCounts[h] = 0;

        foreach (var reservation in reservations)
        {
            // Convert UTC to court's local time using the court's timezone
            var tz = TimeZoneInfo.FindSystemTimeZoneById(court.TimeZone);
            var localStart = TimeZoneInfo.ConvertTimeFromUtc(reservation.StartTimeUtc, tz);
            var localEnd = TimeZoneInfo.ConvertTimeFromUtc(reservation.EndTimeUtc, tz);

            for (var h = localStart.Hour; h < localEnd.Hour; h++)
            {
                if (slotCounts.TryGetValue(h, out var count))
                    slotCounts[h] = count + 1;
            }
        }

        if (slotCounts.Values.All(v => v == 0))
            return null;

        var peakHour = slotCounts.MaxBy(kv => kv.Value).Key;

        // Clamp so that peak-1h and peak+1h stay within opening hours
        var clampedHour = Math.Max(openHour + 1, Math.Min(peakHour, closeHour - 1));

        return new TimeOnly(clampedHour, 0);
    }
}
