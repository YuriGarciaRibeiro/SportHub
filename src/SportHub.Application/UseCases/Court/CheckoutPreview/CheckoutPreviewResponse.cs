namespace Application.UseCases.Court.CheckoutPreview;

public record CheckoutPreviewResponse(
    string Date,
    string StartTime,
    string EndTime,
    int DurationMinutes,
    // Preço efetivo (normal se tudo fora do pico, peak se tudo no pico, null se misto)
    decimal? EffectivePricePerHour,
    bool IsPeakHours,
    decimal? PeakPricePerHour,
    // Breakdown proporcional
    int NormalSlots,
    int PeakSlots,
    decimal NormalSubtotal,
    decimal PeakSubtotal,
    decimal Subtotal,
    decimal ServiceFee,
    decimal Total
);
