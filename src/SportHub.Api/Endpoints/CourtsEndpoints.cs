using Application.UseCases.Court.GetAvailability;
using MediatR;
using WebApi.Extensions.ResultExtensions;

namespace SportHub.Api.Endpoints;

public static class CourtsEndpoints
{
    public static void MapCourtsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/courts")
            .WithTags("Courts");

        group.MapGet("/{courtId}/availability/{date:datetime}", async (
            ISender sender,
            Guid courtId,
            DateTime date
        ) =>
        {
            var query = new GetAvailabilityQuery
            {
                CourtId = courtId,
                Date = date
            };

            var result = await sender.Send(query);

            return result.ToIResult();
        })
        .WithName("GetCourtAvailability")
        .WithSummary("Get availability for a specific court on a given date")
        .WithDescription("Returns a list of available time slots for the specified court on the specified date.")
        .Produces<List<DateTime>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

    }
}