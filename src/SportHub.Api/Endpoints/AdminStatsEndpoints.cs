using Application.Common.Interfaces;
using Application.UseCases.Admin.GetAdminStats;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebApi.Extensions.ResultExtensions;

namespace SportHub.Api.Endpoints;

public static class AdminStatsEndpoints
{
    public static void MapAdminStatsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/admin/stats")
            .WithTags("Admin")
            .RequireAuthorization();

        // GET /admin/stats — Retorna KPIs do dashboard para o tenant atual
        group.MapGet("/", async (ISender sender) =>
        {
            var result = await sender.Send(new GetAdminStatsQuery());
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.ToIResult();
        })
        .WithName("GetAdminStats")
        .WithSummary("Retorna KPIs do dashboard para o tenant atual")
        .Produces<AdminStatsResponse>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);
    }
}
