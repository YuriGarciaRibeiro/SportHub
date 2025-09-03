using Application.Common.Interfaces.Establishments;
using Application.CQRS;

namespace Application.UseCases.Establishments.GetEstablishmentsNearby;

public record GetEstablishmentsNearbyQuery(
    double Latitude,
    double Longitude,
    double RadiusKm = 5.0
) : IQuery<GetEstablishmentsNearbyResponse>;

public record GetEstablishmentsNearbyResponse(
    IEnumerable<EstablishmentLocationDto> Establishments
);

public record EstablishmentLocationDto(
    Guid Id,
    string Name,
    string Description,
    AddressDto Address,
    string? ImageUrl,
    double DistanceKm
);
