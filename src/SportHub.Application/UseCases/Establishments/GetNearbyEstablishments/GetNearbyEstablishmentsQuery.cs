using Application.Common.Interfaces.Establishments;
using Application.CQRS;

namespace Application.UseCases.Establishments.GetNearbyEstablishments;

public record GetNearbyEstablishmentsQuery(
    double Latitude,
    double Longitude,
    double RadiusKm = 5.0
) : IQuery<GetNearbyEstablishmentsResponse>;
