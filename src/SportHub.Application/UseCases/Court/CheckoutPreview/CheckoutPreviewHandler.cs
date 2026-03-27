using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.CQRS;
using Application.Services;

namespace Application.UseCases.Court.CheckoutPreview;

public class CheckoutPreviewHandler : IQueryHandler<CheckoutPreviewQuery, CheckoutPreviewResponse>
{
    private readonly ICourtsRepository _courtsRepository;
    private readonly ITenantContext _tenantContext;

    private static readonly TimeZoneInfo BrazilTz = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    public CheckoutPreviewHandler(ICourtsRepository courtsRepository, ITenantContext tenantContext)
    {
        _courtsRepository = courtsRepository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<CheckoutPreviewResponse>> Handle(CheckoutPreviewQuery request, CancellationToken cancellationToken)
    {
        var court = await _courtsRepository.GetByIdAsync(request.CourtId, new GetCourtIncludeSettings
        {
            IncludeSports = false,
            IncludeLocation = false,
            IncludeTenant = true,
            AsNoTracking = true
        });

        if (court is null)
            return Result.Fail(new NotFound($"Quadra com ID {request.CourtId} não encontrada."));

        var startUtc = request.StartTimeUtc;
        var endUtc = request.EndTimeUtc;
        var durationMinutes = (int)(endUtc - startUtc).TotalMinutes;

        var pricing = PricingCalculator.Calculate(court, _tenantContext.PeakHoursEnabled, startUtc, endUtc);

        var serviceFee = 0m;
        var total = pricing.Subtotal + serviceFee;

        // EffectivePricePerHour: preço único se tudo for do mesmo tipo, null se misto
        decimal? effectivePricePerHour = pricing.NormalSlots == 0
            ? court.PeakPricePerHour ?? court.PricePerHour
            : pricing.PeakSlots == 0
                ? court.PricePerHour
                : null;

        var localStartDisplay = TimeZoneInfo.ConvertTimeFromUtc(startUtc, BrazilTz);
        var localEndDisplay = TimeZoneInfo.ConvertTimeFromUtc(endUtc, BrazilTz);

        return Result.Ok(new CheckoutPreviewResponse(
            Date: localStartDisplay.ToString("yyyy-MM-dd"),
            StartTime: localStartDisplay.ToString("HH:mm"),
            EndTime: localEndDisplay.ToString("HH:mm"),
            DurationMinutes: durationMinutes,
            EffectivePricePerHour: effectivePricePerHour,
            IsPeakHours: pricing.IsPeakHours,
            PeakPricePerHour: court.PeakPricePerHour,
            NormalSlots: pricing.NormalSlots,
            PeakSlots: pricing.PeakSlots,
            NormalSubtotal: pricing.NormalSubtotal,
            PeakSubtotal: pricing.PeakSubtotal,
            Subtotal: pricing.Subtotal,
            ServiceFee: serviceFee,
            Total: total
        ));
    }
}
