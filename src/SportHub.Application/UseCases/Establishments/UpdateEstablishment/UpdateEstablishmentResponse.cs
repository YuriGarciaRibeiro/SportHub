namespace Application.UseCases.Establishments.UpdateEstablishment;

public record UpdateEstablishmentResponse(
    Guid Id,
    string Name,
    string Description,
    AddressResponse Address,
    string ImageUrl
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