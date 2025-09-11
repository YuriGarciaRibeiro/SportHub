namespace Application.Common.Interfaces.Courts;

public record CourtCompleteDto(
    Guid Id,
    string Name,
    int MinBookingSlots,
    int MaxBookingSlots,
    int SlotDurationMinutes,
    TimeOnly OpeningTime,
    TimeOnly ClosingTime,
    decimal PricePerSlot,
    EstablishmentSummaryDto Establishment,
    IEnumerable<SportDto> Sports
);

public record CourtWithSportsDto(
    Guid Id,
    string Name,
    int MinBookingSlots,
    int MaxBookingSlots,
    int SlotDurationMinutes,
    TimeOnly OpeningTime,
    TimeOnly ClosingTime,
    decimal PricePerSlot,
    IEnumerable<SportDto> Sports
);

public record CourtFilterResultDto(
    Guid Id,
    string Name,
    int MinBookingSlots,
    int MaxBookingSlots,
    int SlotDurationMinutes,
    TimeOnly OpeningTime,
    TimeOnly ClosingTime,
    decimal PricePerSlot,
    EstablishmentSummaryDto Establishment,
    IEnumerable<SportDto> Sports
);

public record EstablishmentSummaryDto(
    Guid Id,
    string Name,
    string Description,
    string ImageUrl
);

public record SportDto(
    Guid Id,
    string Name,
    string Description
);
