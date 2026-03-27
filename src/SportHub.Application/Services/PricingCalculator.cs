using Domain.Entities;

namespace Application.Services;

public static class PricingCalculator
{
    private static readonly TimeZoneInfo BrazilTz = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    /// <summary>
    /// Calcula o preço proporcional de uma reserva, slot a slot.
    /// Slots que se sobrepõem ao horário de pico são cobrados na tarifa pico;
    /// os demais na tarifa normal.
    /// </summary>
    public static PricingResult Calculate(Court court, bool peakHoursEnabled, DateTime startUtc, DateTime endUtc)
    {
        var slotDuration = TimeSpan.FromMinutes(court.SlotDurationMinutes);
        var normalPrice = court.PricePerHour;
        var peakPrice = court.PeakPricePerHour ?? court.PricePerHour;

        var hasPeakConfig = peakHoursEnabled
            && court.PeakStartTime.HasValue
            && court.PeakEndTime.HasValue;

        var peakStart = hasPeakConfig ? court.PeakStartTime!.Value.ToTimeSpan() : TimeSpan.Zero;
        var peakEnd = hasPeakConfig ? court.PeakEndTime!.Value.ToTimeSpan() : TimeSpan.Zero;

        var normalSlots = 0;
        var peakSlots = 0;

        for (var slotStart = startUtc; slotStart < endUtc; slotStart += slotDuration)
        {
            var slotEnd = slotStart + slotDuration;

            if (hasPeakConfig)
            {
                var localStart = TimeZoneInfo.ConvertTimeFromUtc(slotStart, BrazilTz).TimeOfDay;
                var localEnd = TimeZoneInfo.ConvertTimeFromUtc(slotEnd, BrazilTz).TimeOfDay;

                // Slot é pico se o início cai dentro do intervalo de pico
                if (localStart >= peakStart && localStart < peakEnd)
                    peakSlots++;
                else
                    normalSlots++;
            }
            else
            {
                normalSlots++;
            }
        }

        var normalSubtotal = normalPrice * normalSlots;
        var peakSubtotal = peakPrice * peakSlots;
        var subtotal = normalSubtotal + peakSubtotal;

        var isPeakHours = peakSlots > 0;

        return new PricingResult(
            NormalSlots: normalSlots,
            PeakSlots: peakSlots,
            NormalSubtotal: normalSubtotal,
            PeakSubtotal: peakSubtotal,
            Subtotal: subtotal,
            IsPeakHours: isPeakHours
        );
    }
}

public record PricingResult(
    int NormalSlots,
    int PeakSlots,
    decimal NormalSubtotal,
    decimal PeakSubtotal,
    decimal Subtotal,
    bool IsPeakHours
);
