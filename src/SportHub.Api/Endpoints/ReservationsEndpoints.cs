using Application.Security;
using Application.UseCases.Reservations.CancelReservation;
using Application.UseCases.Reservations.GetCourtReservations;
using Application.UseCases.Reservations.GetMyReservations;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebApi.Extensions.ResultExtensions;

namespace SportHub.Api.Endpoints;

public static class ReservationsEndpoints
{
    public static void MapReservationsEndpoints(this IEndpointRouteBuilder app)
    {
        // GET /api/reservations/me — reservas do usuário autenticado
        app.MapGet("/api/reservations/me", async (ISender sender) =>
        {
            var result = await sender.Send(new GetMyReservationsQuery());
            return result.ToIResult();
        })
        .WithName("GetMyReservations")
        .WithSummary("Lista as reservas do usuário autenticado")
        .WithTags("Reservations")
        .RequireAuthorization()
        .Produces<List<ReservationResponse>>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);

        // GET /api/courts/{courtId}/reservations — reservas de uma quadra (Staff+)
        app.MapGet("/api/courts/{courtId:guid}/reservations", async (
            Guid courtId,
            DateTime? date,
            ISender sender) =>
        {
            var result = await sender.Send(new GetCourtReservationsQuery(courtId, date));
            return result.ToIResult();
        })
        .WithName("GetCourtReservations")
        .WithSummary("Lista as reservas de uma quadra (Staff/Manager/Owner)")
        .WithTags("Reservations")
        .RequireAuthorization(PolicyNames.IsStaff)
        .Produces<List<ReservationResponse>>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        // DELETE /api/courts/{courtId}/reservations/{id} — cancelar reserva
        app.MapDelete("/api/courts/{courtId:guid}/reservations/{id:guid}", async (
            Guid courtId,
            Guid id,
            ISender sender) =>
        {
            var result = await sender.Send(new CancelReservationCommand(id));
            return result.ToIResult();
        })
        .WithName("CancelReservation")
        .WithSummary("Cancela uma reserva (própria ou por Manager/Owner)")
        .WithTags("Reservations")
        .RequireAuthorization()
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }
}
