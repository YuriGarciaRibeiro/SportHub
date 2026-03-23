using Application.CQRS;

namespace Application.UseCases.Location.GetAllLocations;

public record GetAllLocationsQuery : IQuery<List<LocationResponse>>;
