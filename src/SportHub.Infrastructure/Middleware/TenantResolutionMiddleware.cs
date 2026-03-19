using Application.Common.Enums;
using Application.Common.Interfaces;
using Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace Infrastructure.Middleware;

/// <summary>
/// Middleware que resolve o tenant a partir do subdomínio antes de qualquer handler.
///
/// Ordem de resolução:
/// 1. Subdomínio do Host header (produção): abc.sporthub.app → "abc"
/// 2. Header X-Tenant-Slug (fallback para desenvolvimento local)
///
/// Rotas excluídas do middleware (passam direto):
/// - /api/tenants/** (gestão de tenants — só SuperAdmin)
/// - /health
/// - /scalar/** e /openapi/** (documentação)
/// </summary>
public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    private static readonly string[] _bypassPaths =
    [
        "/api/tenants",
        "/health",
        "/scalar",
        "/openapi",
        "/auth/superadmin",
        "/auth/register",
        "/auth/me",
        "/hubs"
    ];

    /// <summary>
    /// Rotas que tentam resolver o tenant mas não bloqueiam se não encontrarem (ex: login do super admin).
    /// </summary>
    private static readonly string[] _optionalTenantPaths =
    [
        "/auth/login",
        "/auth/refresh",
        "/api/branding"
    ];

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IServiceProvider services)
    {
        var path = context.Request.Path.Value ?? "";

        if (_bypassPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        // Para rotas como /auth/login e /api/branding: tentamos resolver o tenant,
        // mas não bloqueamos se não existir (permite login do Super Admin sem tenant).
        if (_optionalTenantPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        {
            await TryResolveTenantAsync(context, services);
            await _next(context);
            return;
        }

        var slug = ExtractSlug(context);

        if (string.IsNullOrWhiteSpace(slug))
        {
            await WriteProblemAsync(context, 400, "Tenant não identificado.",
                "Acesse via subdomínio (ex: abc.sporthub.app) ou informe o header X-Tenant-Slug.");
            return;
        }

        var tenant = await ResolveTenantBySlugAsync(slug, services);

        if (tenant is null)
        {
            await WriteProblemAsync(context, 404, "Tenant não encontrado.",
                $"Nenhum tenant com slug '{slug}' foi encontrado.");
            return;
        }

        if (tenant.Status == TenantStatus.Suspended)
        {
            await WriteProblemAsync(context, 403, "Tenant suspenso.",
                "Este tenant está temporariamente suspenso. Entre em contato com o suporte.");
            return;
        }

        if (tenant.Status == TenantStatus.Canceled)
        {
            await WriteProblemAsync(context, 410, "Tenant cancelado.",
                "Este tenant foi encerrado.");
            return;
        }

        var tenantContext = services.GetRequiredService<ITenantContext>();
        tenantContext.Resolve(tenant);

        await _next(context);
    }

    private static async Task TryResolveTenantAsync(HttpContext context, IServiceProvider services)
    {
        var slug = ExtractSlug(context);
        if (string.IsNullOrWhiteSpace(slug)) return;

        var tenant = await ResolveTenantBySlugAsync(slug, services);
        if (tenant is null || tenant.Status != TenantStatus.Active) return;

        var tenantContext = services.GetRequiredService<ITenantContext>();
        tenantContext.Resolve(tenant);
    }

    private static async Task<Domain.Entities.Tenant?> ResolveTenantBySlugAsync(string slug, IServiceProvider services)
    {
        var cache = services.GetRequiredService<ICacheService>();
        var cacheKey = cache.GenerateCacheKey(CacheKeyPrefix.TenantBySlug, slug);

        var tenant = await cache.GetAsync<Domain.Entities.Tenant>(cacheKey);

        if (tenant is null)
        {
            var repo = services.GetRequiredService<ITenantRepository>();
            tenant = await repo.GetBySlugAsync(slug);

            if (tenant is not null)
                await cache.SetAsync(cacheKey, tenant, TimeSpan.FromHours(1));
        }

        return tenant;
    }

    private static string? ExtractSlug(HttpContext context)
    {
        var host = context.Request.Host.Host;
        var parts = host.Split('.');

        if (parts.Length >= 3)
            return parts[0].ToLowerInvariant();

        if (context.Request.Headers.TryGetValue("X-Tenant-Slug", out var headerSlug))
            return headerSlug.ToString().ToLowerInvariant();

        return null;
    }

    private static async Task WriteProblemAsync(HttpContext context, int status, string title, string detail)
    {
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";

        var problem = new
        {
            type = $"https://httpstatuses.io/{status}",
            title,
            status,
            detail
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}
