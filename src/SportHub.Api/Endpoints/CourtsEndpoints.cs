using Application.Security;
using Application.UseCases.Court.GetAvailability;
using Application.UseCases.Reservations.CreateReservation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
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
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        // POST /courts/{courtId}/reservations

        group.MapPost("/courts/{courtId:guid}/reservations", async (
            Guid courtId,
            ReservationRequest request,
            ISender sender) =>
        {
            var command = new CreateReservationCommand
            {
                CourtId = courtId,
                Reservation = request
            };

            var result = await sender.Send(command);

            return result.ToIResult(StatusCodes.Status201Created);
        })
        .WithName("CreateCourtReservation")
        .WithSummary("Create a reservation for a court")
        .WithDescription("Creates a reservation for the specified court.")
        .Produces<CreateReservationResponse>(StatusCodes.Status201Created)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
        .RequireAuthorization();
    }
}