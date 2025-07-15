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

        return app;
    }
}