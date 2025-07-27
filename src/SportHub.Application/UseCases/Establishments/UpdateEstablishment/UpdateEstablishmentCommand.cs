using Application.CQRS;

namespace Application.UseCases.Establishments.UpdateEstablishment;

public record UpdateEstablishmentCommand(
    Guid Id,
    UpdateEstablishmentRequest Request
) : ICommand<UpdateEstablishmentResponse>;

public record UpdateEstablishmentRequest(
    string Name,
    string Description,
    AddressResponse Address,
    string ImageUrl
);

public record AddressRequest(
    string Street,
    string Number,
    string? Complement,
    string Neighborhood,
    string City,
    string State,
    string ZipCode
);