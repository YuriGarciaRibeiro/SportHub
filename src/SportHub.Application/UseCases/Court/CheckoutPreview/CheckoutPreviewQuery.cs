using Application.CQRS;

namespace Application.UseCases.Court.CheckoutPreview;

public record CheckoutPreviewQuery(
    Guid CourtId,
    DateTime StartTimeUtc,
    DateTime EndTimeUtc
) : IQuery<CheckoutPreviewResponse>;
