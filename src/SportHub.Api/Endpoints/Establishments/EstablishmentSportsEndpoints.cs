using Application.UseCases.Establishments.GetEstablishmentSports;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebApi.Extensions.ResultExtensions;

namespace SportHub.Api.Endpoints.Establishments;

public static class EstablishmentSportsEndpoints
{
    public static RouteGroupBuilder MapEstablishmentSportsEndpoints(this RouteGroupBuilder group)
    {
        // GET /establishments/{establishmentId}/sports
        group.MapGet("/{establishmentId:guid}/sports", async (
            Guid establishmentId,
            ISender sender) =>
        {
            var result = await sender.Send(new GetEstablishmentSportsQuery { EstablishmentId = establishmentId });

            return result.ToIResult();
        })
        .WithName("GetEstablishmentSports")
        .WithSummary("Get sports by establishment ID")
        .WithDescription("Retrieves a list of sports associated with the specified establishment.")
        .Produces<GetEstablishmentSportsResponse>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
        .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        return group;
    }
}