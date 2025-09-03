namespace Application.UseCases.Establishments.GetEstablishmentById;

public record GetEstablishmentByIdResponse(
    Guid Id,
    string Name,
    string Description,
    string PhoneNumber,
    string Email,
    string Website,
    TimeOnly OpeningTime,
    TimeOnly ClosingTime,
    AddressResponse Address,
    string? ImageUrl,
    bool? IsFavorite,
    IEnumerable<EstablishmentUserResponse> Users,
    IEnumerable<CourtResponse> Courts,
    IEnumerable<SportResponse> Sports
);

public record EstablishmentUserResponse(
    Guid UserId,
    string FirstName,
    string LastName,
    string Email,
    string Role
);

public record AddressResponse(
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

public record CourtResponse(
    Guid Id,
    string Name,
    int SlotDurationMinutes,
    int MinBookingSlots,
    int MaxBookingSlots,
    string TimeZone,
    decimal PricePerSlot,
    IEnumerable<SportResponse> Sports
);

public record SportResponse(
    Guid Id,
    string Name,
    string Description
);