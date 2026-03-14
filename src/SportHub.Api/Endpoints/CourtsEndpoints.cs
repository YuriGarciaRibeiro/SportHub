using Application.Common.Interfaces;
using Application.UseCases.Court.CreateCourt;
using Application.UseCases.Court.GetAllCourts;
using Application.UseCases.Court.GetCourtById;
using Application.UseCases.Court.GetAvailability;
using Application.UseCases.Reservations.CreateReservation;
using Application.CQRS;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebApi.Extensions.ResultExtensions;

namespace SportHub.Api.Endpoints;

public static class CourtsEndpoints
{
    public static void MapCourtsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/courts")
            .WithTags("Courts");

        // GET /courts — List all courts for the current tenant (public)
        group.MapGet("/", async (ISender sender) =>
        {
            var result = await sender.Send(new GetAllCourtsQuery());
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.ToIResult();
        })
        .WithName("GetCourts")
        .WithSummary("Lista todas as quadras do tenant atual (público)")
        .AllowAnonymous()
        .Produces<IEnumerable<CourtPublicResponse>>(StatusCodes.Status200OK);

        // GET /courts/{id} — Obter uma quadra específica do tenant
        group.MapGet("/{id:guid}", async (Guid id, ISender sender) =>
        {
            var query = new GetCourtByIdQuery(id);
            var result = await sender.Send(query);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.ToIResult();
        })
        .WithName("GetCourtById")
        .WithSummary("Obtém os detalhes de uma quadra específica no tenant atual")
        .RequireAuthorization()
        .Produces<CourtPublicResponse>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        // POST /courts — Create a new court (requires auth)
        group.MapPost("/", async (
            CourtRequest request,
            ISender sender) =>
        {
            var command = new CreateCourtCommand(request);
            var result = await sender.Send(command);
            return result.IsSuccess
                ? Results.StatusCode(StatusCodes.Status201Created)
                : result.ToIResult();
        })
        .WithName("CreateCourt")
        .WithSummary("Cria uma nova quadra no tenant atual")
        .RequireAuthorization()
        .Produces(StatusCodes.Status201Created)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status422UnprocessableEntity);

        // GET /courts/{courtId}/availability/{date}
        group.MapGet("/{courtId}/availability/{date:datetime}", async (
            ISender sender,
            Guid courtId,
            DateTime date) =>
        {
            var query = new GetAvailabilityQuery { CourtId = courtId, Date = date };
            var result = await sender.Send(query);
            return result.ToIResult();
        })
        .WithName("GetCourtAvailability")
        .WithSummary("Get availability for a specific court on a given date")
        .AllowAnonymous()
        .Produces<List<DateTime>>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        // POST /courts/{courtId}/reservations
        group.MapPost("/{courtId:guid}/reservations", async (
            Guid courtId,
            ReservationRequest request,
            ISender sender) =>
        {
            var command = new CreateReservationCommand { CourtId = courtId, Reservation = request };
            var result = await sender.Send(command);
            return result.ToIResult(StatusCodes.Status201Created);
        })
        .WithName("CreateCourtReservation")
        .WithSummary("Create a reservation for a court")
        .RequireAuthorization()
        .Produces<CreateReservationResponse>(StatusCodes.Status201Created)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }
}