using AspNetCoreRateLimit;

namespace Api.Extensions.Security;

public static class RateLimitExtensions
{
    public static WebApplicationBuilder AddRateLimit(this WebApplicationBuilder builder)
    {
        // Configuração necessária para o Rate Limiting
        builder.Services.AddMemoryCache();
        builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
        
        // Registrar os serviços necessários
        builder.Services.AddInMemoryRateLimiting();
        builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

        return builder;
    }

    public static WebApplication UseRateLimit(this WebApplication app)
    {
        app.UseIpRateLimiting();
        return app;
    }
}
