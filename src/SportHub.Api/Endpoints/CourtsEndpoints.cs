using Application.Common.QueryFilters;
using Application.Security;
using Application.UseCases.Court.GetAvailability;
using Application.UseCases.Court.GetCourt;
using Application.UseCases.Court.GetCourts;
using Application.UseCases.Court.UpdateCourt;
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

        // GET /courts
        group.MapGet("/", async (ISender sender, [AsParameters] CourtQueryFilter filter) =>
        {
            var query = new GetCourtsQuery
            {
                Filter = filter
            };

            var result = await sender.Send(query);

            return result.ToIResult();
        })
        .WithName("GetCourts")
        .WithSummary("Get a list of courts")
        .WithDescription("Returns a list of courts based on the provided filter.")
        .Produces<GetCourtsResponse>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        // GET /courts/{courtId}
        group.MapGet("/{courtId:guid}", async (ISender sender, Guid courtId) =>
        {
            var query = new GetCourtQuery
            {
                CourtId = courtId
            };

            var result = await sender.Send(query);

            return result.ToIResult();
        })
        .WithName("GetCourt")
        .WithSummary("Get a specific court")
        .WithDescription("Returns the details of a specific court.")
        .Produces<GetCourtResponse>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        // GET /courts/{courtId}/availability/{date:datetime}
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
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        // POST /courts/{courtId}/reservations
        group.MapPost("/{courtId:guid}/reservations", async (
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