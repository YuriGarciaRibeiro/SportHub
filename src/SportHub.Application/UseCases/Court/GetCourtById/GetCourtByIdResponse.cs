namespace Application.UseCases.Court.GetCourtById;

public record CourtPublicResponse(
    Guid Id,
    string Name,
    string? ImageUrl,
    List<string> ImageUrls,
    decimal PricePerHour,
    int SlotDurationMinutes,
    string OpensAt,
    string ClosesAt,
    List<string> Amenities,
    List<SportSummary> Sports,
    Guid? LocationId,
    string? LocationName);

public record SportSummary(Guid Id, string Name);
