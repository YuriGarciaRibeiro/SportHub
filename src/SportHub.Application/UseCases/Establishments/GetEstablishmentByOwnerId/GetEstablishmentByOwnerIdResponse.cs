namespace Application.UseCases.Establishments.GetEstablishmentByOwnerId;

public record GetEstablishmentsByOwnerIdResponse(
    IEnumerable<EstablishmentResponse> Establishments
);

public record EstablishmentResponse(
    Guid Id,
    string Name,
    string Description,
    AddressResponse Address,
    string ImageUrl,
    IEnumerable<EstablishmentUserResponse> Users,
    IEnumerable<CourtResponse> Courts
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
    string SportType,
    int SlotDurationMinutes,
    int MinBookingSlots,
    int MaxBookingSlots,
    string TimeZone
);