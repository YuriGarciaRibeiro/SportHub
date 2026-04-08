using Application.UseCases.CourtMaintenance.GetCourtMaintenances;

namespace Application.UseCases.Court.GetCourtById;

public record CourtPublicResponse(
    Guid Id,
    string Name,
    string? ImageUrl,
    List<string> ImageUrls,
    decimal PricePerHour,
    int SlotDurationMinutes,
    int MinBookingSlots,
    int MaxBookingSlots,
    string OpensAt,
    string ClosesAt,
    List<string> Amenities,
    List<SportSummary> Sports,
    Guid? LocationId,
    string? LocationName,
    decimal? PeakPricePerHour,
    TimeOnly? PeakStartTime,
    TimeOnly? PeakEndTime,
    List<CourtMaintenanceResponse> Maintenances);

public record SportSummary(Guid Id, string Name);
