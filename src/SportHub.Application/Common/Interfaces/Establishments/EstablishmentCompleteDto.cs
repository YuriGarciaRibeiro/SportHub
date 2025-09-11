using Domain.Enums;

namespace Application.Common.Interfaces.Establishments;

public record EstablishmentCompleteDto(
    Guid Id,
    string Name,
    string Description,
    string PhoneNumber,
    string Email,
    string Website,
    TimeOnly OpeningTime,
    TimeOnly ClosingTime,
    string TimeZone,
    AddressDto Address,
    string? ImageUrl,
    IEnumerable<SportDto> Sports,
    IEnumerable<EstablishmentUserDto> Users,
    IEnumerable<CourtDto> Courts,
    double? DistanceKm = null,
    double? AverageRating = null,
    IEnumerable<EvaluationDto> Evaluations = null!
);

public record AddressDto(
    string Street,
    string Number,
    string? Complement,
    string Neighborhood,
    string City,
    string State,
    string ZipCode,
    double? Latitude,
    double? Longitude
);

public record SportDto(
    Guid Id,
    string Name,
    string Description
);

public record EstablishmentUserDto(
    Guid UserId,
    string Name,
    string Email,
    EstablishmentRole Role
);

public record CourtDto(
    Guid Id,
    string Name,
    int MinBookingSlots,
    int MaxBookingSlots,
    int SlotDurationMinutes,
    decimal PricePerSlot,
    IEnumerable<SportDto> Sports
);

public record EvaluationDto(
    Guid Id,
    Guid UserId,
    string UserName,
    int Rating,
    string? Comment,
    DateTime CreatedAt
);
