using Application.Common.Models;
using Application.Security;
using Application.UseCases.Tenant.ActivateTenant;
using Application.UseCases.Tenant.GetAllTenants;
using Application.UseCases.Tenant.GetTenant;
using Application.UseCases.Tenant.ProvisionTenant;
using Application.UseCases.Tenant.SuspendTenant;
using Application.UseCases.Tenant.UpdateTenant;
using Application.UseCases.Tenant.GetTenantUsers;
using Application.UseCases.Tenant.ProvisionTenantOwner;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebApi.Extensions.ResultExtensions;

namespace SportHub.Api.Endpoints;

public static class TenantEndpoints
{
    public static RouteGroupBuilder MapTenantEndpoints(this IEndpointRouteBuilder app)
    {
        // Grupo fora do middleware de tenant — acessível sem subdomínio
        // Protegido por SuperAdmin
        var group = app.MapGroup("/api/tenants")
            .WithTags("Tenants")
            .RequireAuthorization(PolicyNames.IsSuperAdmin);

        // POST /api/tenants — Provisionar novo tenant
        group.MapPost("/", async (
            ProvisionTenantCommand command,
            ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.ToIResult();
        })
        .WithName("ProvisionTenant")
        .WithSummary("Provisiona um novo tenant (cria schema + tabelas + seed)")
        .Produces<ProvisionTenantResponse>(StatusCodes.Status201Created)
        .Produces<ProblemDetails>(StatusCodes.Status409Conflict)
        .Produces<ProblemDetails>(StatusCodes.Status422UnprocessableEntity);

        // GET /api/tenants — Lista todos os tenants
        group.MapGet("/", async (
            [FromQuery] int? page,
            [FromQuery] int? pageSize,
            [FromQuery] string? name,
            [FromQuery] string? slug,
            [FromQuery] TenantStatus? status,
            [FromQuery] string? searchTerm,
            ISender sender) =>
        {
            var filter = new GetTenantsFilter
            {
                Page = page is > 0 ? page.Value : 1,
                PageSize = pageSize is > 0 ? pageSize.Value : 10,
                Name = name,
                Slug = slug,
                Status = status,
                SearchTerm = searchTerm
            };
            var result = await sender.Send(new GetAllTenantsQuery(filter));
            return result.ToIResult();
        })
        .WithName("GetAllTenants")
        .WithSummary("Lista todos os tenants da plataforma com filtros e paginação")
        .Produces<PagedResult<GetAllTenantsResponse>>();

        // GET /api/tenants/{id}
        group.MapGet("/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetTenantQuery(id));
            return result.ToIResult();
        })
        .WithName("GetTenant")
        .WithSummary("Retorna metadados de um tenant pelo id")
        .Produces<GetTenantResponse>()
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        // GET /api/tenants/{id}/users
        group.MapGet("/{id:guid}/users", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetTenantUsersQuery(id));
            return result.ToIResult();
        })
        .WithName("GetTenantUsers")
        .WithSummary("Retorna os usuários específicos de um tenant")
        .Produces<List<GetTenantUsersResponse>>()
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        // POST /api/tenants/{id}/owner
        group.MapPost("/{id:guid}/owner", async (
            Guid id,
            ProvisionTenantOwnerRequest body,
            ISender sender) =>
        {
            var result = await sender.Send(new ProvisionTenantOwnerCommand(id, body.Email));
            return result.ToIResult();
        })
        .WithName("ProvisionTenantOwner")
        .WithSummary("Gera (ou recria) o usuário administrador do tenant vinculando a um e-mail específico.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status422UnprocessableEntity);

        // PATCH /api/tenants/{id}/branding — Atualizar branding
        group.MapPatch("/{id:guid}/branding", async (
            Guid id,
            UpdateTenantBrandingRequest body,
            ISender sender) =>
        {
            var result = await sender.Send(new UpdateTenantCommand(id, body.LogoUrl, body.PrimaryColor));
            return result.ToIResult();
        })
        .WithName("UpdateTenantBranding")
        .WithSummary("Atualiza logo e cor primária do tenant")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status422UnprocessableEntity);

        // POST /api/tenants/{id}/suspend
        group.MapPost("/{id:guid}/suspend", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new SuspendTenantCommand(id));
            return result.ToIResult();
        })
        .WithName("SuspendTenant")
        .WithSummary("Suspende um tenant (403 em todas as requests do subdomínio)")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        // POST /api/tenants/{id}/activate
        group.MapPost("/{id:guid}/activate", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new ActivateTenantCommand(id));
            return result.ToIResult();
        })
        .WithName("ActivateTenant")
        .WithSummary("Reativa um tenant suspenso")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        return group;
    }
}

public record UpdateTenantBrandingRequest(string? LogoUrl, string? PrimaryColor);
public record ProvisionTenantOwnerRequest(string Email);
