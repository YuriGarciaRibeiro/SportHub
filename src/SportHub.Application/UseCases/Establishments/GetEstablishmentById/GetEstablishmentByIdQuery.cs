using Application.CQRS;

namespace Application.UseCases.Establishments.GetEstablishmentById;

public record GetEstablishmentByIdQuery(
    Guid Id, 
    double? Latitude = null, 
    double? Longitude = null
) : IQuery<GetEstablishmentByIdResponse>
{
    public Guid Id { get; init; } = Id;
    public double? Latitude { get; init; } = Latitude;
    public double? Longitude { get; init; } = Longitude;
};
