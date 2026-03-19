using Application.Common.Models;
using Application.Security;
using Application.UseCases.Customers.GetCustomerDetail;
using Application.UseCases.Customers.GetCustomers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebApi.Extensions.ResultExtensions;

namespace SportHub.Api.Endpoints;

public static class CustomersEndpoints
{
    public static void MapCustomersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/customers")
            .WithTags("Customers")
            .RequireAuthorization(PolicyNames.IsStaff);

        // GET /api/customers — Lista clientes com métricas agregadas
        group.MapGet("/", async (
            [AsParameters] GetCustomersFilter filter,
            ISender sender) =>
        {
            var result = await sender.Send(new GetCustomersQuery(filter));
            return result.ToIResult();
        })
        .WithName("GetCustomers")
        .WithSummary("Lista clientes do tenant com métricas de reservas")
        .Produces<PagedResult<CustomerSummaryDto>>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
        .Produces<ProblemDetails>(StatusCodes.Status403Forbidden);

        // GET /api/customers/{id} — Detalhe do cliente
        group.MapGet("/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetCustomerDetailQuery(id));
            return result.ToIResult();
        })
        .WithName("GetCustomerDetail")
        .WithSummary("Retorna detalhes de um cliente com histórico de reservas e quadras favoritas")
        .Produces<CustomerDetailDto>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
        .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }
}
