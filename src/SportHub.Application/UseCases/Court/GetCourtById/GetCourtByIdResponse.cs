namespace Application.UseCases.Court.GetCourtById;

public record CourtPublicResponse(
    Guid Id,
    string Name,
    string? ImageUrl,
    decimal PricePerHour,
    int SlotDurationMinutes,
    string OpensAt,
    string ClosesAt,
    List<SportSummary> Sports);

public record SportSummary(Guid Id, string Name);
