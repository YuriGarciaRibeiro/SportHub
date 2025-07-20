public record GetEstablishmentResponse(
    Guid Id,
    string Name,
    string Description,
    AddressResponse Address,
    string ImageUrl,
    IEnumerable<EstablishmentUserResponse> Users
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