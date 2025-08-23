using Api.Extensions.Application;
using Api.Extensions.Authentication;
using Api.Extensions.Configuration;
using Api.Extensions.Database;
using Api.Extensions.DependencyInjection;
using Api.Extensions.MediatR;

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

        // Endpoints
        app.UseApiEndpoints();

        return app;
    }
}
