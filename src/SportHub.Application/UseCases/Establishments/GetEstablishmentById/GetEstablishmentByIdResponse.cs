namespace Application.UseCases.Establishments.GetEstablishmentById;

public record GetEstablishmentByIdResponse(
    Guid Id,
    string Name,
    string Description,
    AddressResponse Address,
    string ImageUrl,
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
    string ZipCode
);

public record CourtResponse(
    Guid Id,
    string Name,
    int SlotDurationMinutes,
    int MinBookingSlots,
    int MaxBookingSlots,
    string TimeZone,
    IEnumerable<SportResponse> Sports
);

public record SportResponse(
    Guid Id,
    string Name,
    string Description
);