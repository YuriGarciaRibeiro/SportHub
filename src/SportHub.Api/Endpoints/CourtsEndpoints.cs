using Application.Common.Interfaces;
using Application.Common.Models;
using Application.UseCases.Court.CreateCourt;
using Application.UseCases.Court.DeleteCourt;
using Application.UseCases.Court.GetAllCourts;
using Application.UseCases.Court.GetCourtById;
using Application.UseCases.Court.GetAvailability;
using Application.UseCases.Court.UpdateCourt;
using Application.UseCases.Court.UploadCourtImage;
using Application.UseCases.Reservations.CreateReservation;
using Application.CQRS;
using Application.Security;
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
        group.MapGet("/", async (
            [FromQuery] int? page,
            [FromQuery] int? pageSize,
            [FromQuery] string? name,
            [FromQuery] Guid? sportId,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] string? searchTerm,
            ISender sender) =>
        {
            var filter = new GetCourtsFilter
            {
                Page = page is > 0 ? page.Value : 1,
                PageSize = pageSize is > 0 ? pageSize.Value : 10,
                Name = name,
                SportId = sportId,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                SearchTerm = searchTerm
            };
            var result = await sender.Send(new GetAllCourtsQuery(filter));
            return result.ToIResult();
        })
        .WithName("GetCourts")
        .WithSummary("Lista todas as quadras do tenant atual com filtros e paginação (público)")
        .AllowAnonymous()
        .Produces<PagedResult<CourtPublicResponse>>(StatusCodes.Status200OK);

        // GET /courts/{id} — Obter uma quadra específica do tenant
        group.MapGet("/{id:guid}", async (Guid id, ISender sender) =>
        {
            var query = new GetCourtByIdQuery(id);
            var result = await sender.Send(query);
            return result.ToIResult();
        })
        .WithName("GetCourtById")
        .WithSummary("Obtém os detalhes de uma quadra específica no tenant atual")
        .RequireAuthorization()
        .Produces<CourtPublicResponse>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        // POST /courts — IsManager
        group.MapPost("/", async (
            CourtRequest request,
            ISender sender) =>
        {
            var command = new CreateCourtCommand(request);
            var result = await sender.Send(command);
            return result.IsSuccess
                ? Results.Created($"/api/courts/{result.Value}", new { id = result.Value })
                : result.ToIResult();
        })
        .WithName("CreateCourt")
        .WithSummary("Cria uma nova quadra no tenant atual")
        .RequireAuthorization(PolicyNames.IsManager)
        .Produces<object>(StatusCodes.Status201Created)
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

        // PUT /courts/{id} — IsManager
        group.MapPut("/{id:guid}", async (Guid id, CourtRequest request, ISender sender) =>
        {
            var command = new UpdateCourtCommand { Id = id, Court = request };
            var result = await sender.Send(command);
            return result.ToIResult();
        })
        .WithName("UpdateCourt")
        .WithSummary("Atualiza os dados de uma quadra")
        .RequireAuthorization(PolicyNames.IsManager)
        .Produces<CourtPublicResponse>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status422UnprocessableEntity);

        // DELETE /courts/{id} — IsManager
        group.MapDelete("/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new DeleteCourtCommand(id));
            return result.ToIResult();
        })
        .WithName("DeleteCourt")
        .WithSummary("Remove uma quadra (soft delete)")
        .RequireAuthorization(PolicyNames.IsManager)
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        // POST /courts/{courtId}/upload-image — IsManager
        group.MapPost("/{courtId:guid}/upload-image", async (
            Guid courtId,
            IFormFile file,
            ISender sender) =>
        {
            if (file is null || file.Length == 0)
                return Results.UnprocessableEntity(new { detail = "Nenhum arquivo enviado." });

            await using var stream = file.OpenReadStream();

            var command = new UploadCourtImageCommand(
                courtId,
                stream,
                file.FileName,
                file.ContentType,
                file.Length);

            var result = await sender.Send(command);
            return result.ToIResult();
        })
        .WithName("UploadCourtImage")
        .WithSummary("Faz upload de imagem para a quadra e atualiza ImageUrl")
        .RequireAuthorization(PolicyNames.IsManager)
        .DisableAntiforgery()
        .Accepts<IFormFile>("multipart/form-data")
        .Produces<UploadCourtImageResponse>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status422UnprocessableEntity);
    }
}