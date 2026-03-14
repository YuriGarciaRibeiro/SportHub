using SportHub.Api.Endpoints;

namespace WebAPI.Extensions;

public static class AppExtensions
{
    public static WebApplication UseMiddlewares(this WebApplication app)
    {
        return app;
    }

    public static WebApplication UseEndpoints(this WebApplication app)
    {
        app.MapAuthEndpoints();
        app.MapSportsEndpoints();
        app.MapCourtsEndpoints();
        app.MapReservationsEndpoints();
        app.MapAdminStatsEndpoints();
        app.MapMembersEndpoints();

        // Tenant endpoints (fora do middleware de tenant — acessível sem subdomínio)
        app.MapTenantEndpoints();

        // Endpoints tenant-scoped (acessados via subdomínio)
        app.MapBrandingEndpoints();
        app.MapSettingsEndpoints();

        return app;
    }
}