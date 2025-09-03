using Application.UseCases.Establishments.GetNearbyEstablishments;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Api.Extensions.Results;

namespace SportHub.Api.Endpoints.Establishments;

public static class EstablishmentLocationEndpoints
{
    public static RouteGroupBuilder MapEstablishmentLocationEndpoints(this RouteGroupBuilder group)
    {
        // GET /establishments/nearby?latitude={lat}&longitude={lng}&radiusKm={radius}
        group.MapGet("/nearby", async (
            [FromQuery] double latitude,
            [FromQuery] double longitude,
            [FromQuery] double radiusKm,
            ISender sender) =>
        {
            var query = new GetNearbyEstablishmentsQuery(latitude, longitude, radiusKm);
            var result = await sender.Send(query);
            
            return result.ToIResult();
        })
        .WithName("GetNearbyEstablishments")
        .WithSummary("Get establishments near a location")
        .WithDescription("Retrieves establishments within a specified radius from the given coordinates, ordered by distance.")
        .Produces<GetNearbyEstablishmentsResponse>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        return group;
    }
}