using Domain.Enums;

namespace Application.Common.Interfaces.Establishments;

public record EstablishmentCompleteDto(
    Guid Id,
    string Name,
    string Description,
    AddressDto Address,
    string? ImageUrl,
    IEnumerable<SportDto> Sports,
    IEnumerable<EstablishmentUserDto> Users,
    IEnumerable<CourtDto> Courts
);

public record AddressDto(
    string Street,
    string Number,
    string? Complement,
    string Neighborhood,
    string City,
    string State,
    string ZipCode
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
    string TimeZone,
    IEnumerable<SportDto> Sports
);
