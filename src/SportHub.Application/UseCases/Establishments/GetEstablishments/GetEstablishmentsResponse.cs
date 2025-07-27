namespace Application.UseCases.Establishments.GetEstablishments;

public class GetEstablishmentsResponse
{
    public IEnumerable<EstablishmentResponse> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
}

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