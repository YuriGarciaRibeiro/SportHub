using Application.Common.QueryFilters;
using Application.Security;
using Application.UseCases.Establishments.GetEstablishmentReservations;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Api.Extensions.Results;

namespace SportHub.Api.Endpoints.Establishments;

public static class EstablishmentReservationsEndpoints
{
    public static RouteGroupBuilder MapEstablishmentReservationsEndpoints(this RouteGroupBuilder group)
    {
        // GET /establishments/{establishmentId}/reservations
        group.MapGet("/{establishmentId:guid}/reservations", async (
            Guid establishmentId,
            [AsParameters] EstablishmentReservationsQueryFilter filter,
            ISender sender) =>
        {
            var result = await sender.Send(new GetEstablishmentReservationsQuery { EstablishmentId = establishmentId, Filter = filter });

            return result.ToIResult();
        })
        .WithName("GetEstablishmentReservations")
        .WithSummary("Get reservations by establishment ID")
        .WithDescription("Retrieves a list of reservations associated with the specified establishment.")
        .Produces<GetEstablishmentReservationsResponse>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
        .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
        .RequireAuthorization(PolicyNames.IsEstablishmentManager);

        return group;
    }
}
