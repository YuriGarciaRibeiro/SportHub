using Application.Common.Interfaces;

namespace SportHub.Api.Endpoints;

public static class BrandingEndpoints
{
    public static IEndpointRouteBuilder MapBrandingEndpoints(this IEndpointRouteBuilder app)
    {
        // GET /api/branding — público, sem auth, acessado com subdomínio
        app.MapGet("/api/branding", (ITenantContext tenantCtx) =>
        {
            if (!tenantCtx.IsResolved)
                return Results.NotFound(new { message = "Tenant não encontrado." });

            return Results.Ok(new BrandingResponse(
                tenantCtx.TenantName,
                tenantCtx.LogoUrl,
                tenantCtx.PrimaryColor
            ));
        })
        .WithName("GetBranding")
        .WithSummary("Retorna informações de branding do tenant atual (público, sem auth)")
        .AllowAnonymous()
        .WithTags("Branding")
        .Produces<BrandingResponse>();

        return app;
    }
}

public record BrandingResponse(string Name, string? LogoUrl, string? PrimaryColor);
