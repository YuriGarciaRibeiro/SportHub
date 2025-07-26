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
    IEnumerable<SportResponse> Sports
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

public record SportResponse(
    Guid Id,
    string Name,
    string Description
);