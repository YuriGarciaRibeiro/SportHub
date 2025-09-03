using Application.Common.Interfaces.Establishments;

namespace Application.UseCases.Establishments.GetNearbyEstablishments;

public record GetNearbyEstablishmentsResponse(
    IEnumerable<EstablishmentLocationDto> Establishments
);

public record EstablishmentLocationDto(
    Guid Id,
    string Name,
    string Description,
    AddressDto Address,
    string? ImageUrl,
    double DistanceKm,
    IEnumerable<Application.Common.Interfaces.Establishments.SportDto> Sports
);
