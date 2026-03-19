using Application.Security;
using Application.UseCases.Admin.GetFinanceiro;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebApi.Extensions.ResultExtensions;

namespace SportHub.Api.Endpoints;

public static class FinanceiroEndpoints
{
    public static void MapFinanceiroEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/admin/financeiro")
            .WithTags("Admin")
            .RequireAuthorization(PolicyNames.IsManager);

        // GET /admin/financeiro?year=2026&month=3
        group.MapGet("/", async (
            [FromQuery] int? year,
            [FromQuery] int? month,
            ISender sender) =>
        {
            var now = DateTime.UtcNow;
            var y = year ?? now.Year;
            var m = month ?? now.Month;
            var result = await sender.Send(new GetFinanceiroQuery(y, m));
            return result.ToIResult();
        })
        .WithName("GetFinanceiro")
        .WithSummary("Retorna métricas financeiras detalhadas para o período selecionado")
        .Produces<FinanceiroResponse>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);
    }
}
