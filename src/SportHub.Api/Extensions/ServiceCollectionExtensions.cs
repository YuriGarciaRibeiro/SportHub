using Api.Extensions.Application;
using Api.Extensions.Authentication;
using Api.Extensions.Configuration;
using Api.Extensions.Database;
using Api.Extensions.DependencyInjection;
using Api.Extensions.MediatR;
using Api.Extensions.Security;

namespace Api.Extensions;

/// <summary>
/// Extensões principais para configuração da aplicação
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adiciona todos os serviços necessários para a aplicação
    /// </summary>
    public static WebApplicationBuilder AddAllServices(this WebApplicationBuilder builder)
    {
        // Configurações básicas
        builder.AddSettings();
        builder.AddSerilogLogging();

        // Database e Cache
        builder.AddDatabase(builder.Configuration);
        builder.AddCaching();

        // Serviços de aplicação
        builder.AddApplicationServices();
        builder.AddInfrastructureServices();
        builder.AddRepositories();

        // MediatR e Behaviors
        builder.AddMediatR();
        builder.AddCustomExceptionHandler();

        // Autenticação e Autorização
        builder.AddAuthentication();
        builder.AddAuthorization();

        // Segurança
        builder.AddRateLimit();

        // Seeders
        builder.AddSeeders();

        return builder;
    }

    /// <summary>
    /// Configura todos os middlewares e endpoints da aplicação
    /// </summary>
    public static WebApplication ConfigureApplication(this WebApplication app)
    {
        // Executar migrações
        app.ExecuteMigrations();

        // Middlewares
        app.UseCustomMiddlewares();

        // Rate Limiting (deve vir antes dos endpoints)
        app.UseRateLimit();

        // Endpoints
        app.UseApiEndpoints();

        return app;
    }
}
