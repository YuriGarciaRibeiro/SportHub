using Application.Common.Interfaces;
using Application.UseCases.Location.GetAllLocations;

namespace SportHub.Api.Endpoints;

public static class BrandingEndpoints
{
    public static IEndpointRouteBuilder MapBrandingEndpoints(this IEndpointRouteBuilder app)
    {
        // GET /api/branding — público, sem auth, acessado com subdomínio
        app.MapGet("/api/branding", async (ITenantContext tenantCtx, ILocationsRepository locationsRepo) =>
        {
            if (!tenantCtx.IsResolved)
                return Results.NotFound(new { message = "Tenant não encontrado." });

            var locations = await locationsRepo.GetAllAsync();
            var locationResponses = locations.Select(l => new LocationResponse(
                l.Id,
                l.Name,
                l.Address is null ? null : new AddressResponse(
                    l.Address.Street,
                    l.Address.Number,
                    l.Address.Complement,
                    l.Address.Neighborhood,
                    l.Address.City,
                    l.Address.State,
                    l.Address.ZipCode),
                l.Phone,
                l.BusinessHours.Select(h => new DailyScheduleResponse(h.DayOfWeek, h.IsOpen, h.OpenTime, h.CloseTime)).ToList(),
                l.IsDefault,
                l.InstagramUrl,
                l.FacebookUrl,
                l.WhatsappNumber
            )).ToList();

            var socialMedia = tenantCtx.InstagramUrl is not null
                || tenantCtx.FacebookUrl is not null
                || tenantCtx.WhatsappNumber is not null
                ? new SocialMediaResponse(tenantCtx.InstagramUrl, tenantCtx.FacebookUrl, tenantCtx.WhatsappNumber)
                : null;

            return Results.Ok(new BrandingResponse(
                tenantCtx.TenantName,
                tenantCtx.LogoUrl,
                tenantCtx.CoverImageUrl,
                tenantCtx.PrimaryColor,
                tenantCtx.Tagline,
                tenantCtx.CancelationWindowHours,
                socialMedia,
                locationResponses,
                tenantCtx.PeakHoursEnabled
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

public record BrandingResponse(
    string Name,
    string? LogoUrl,
    string? CoverImageUrl,
    string? PrimaryColor,
    string? Tagline,
    int? CancelationWindowHours,
    SocialMediaResponse? SocialMedia,
    List<LocationResponse> Locations,
    bool PeakHoursEnabled);

public record SocialMediaResponse(string? InstagramUrl, string? FacebookUrl, string? WhatsappNumber);
