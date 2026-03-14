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
        app.MapAdminStatsEndpoints();

        // Tenant endpoints (fora do middleware de tenant — acessível sem subdomínio)
        app.MapTenantEndpoints();

        return app;
    }
}