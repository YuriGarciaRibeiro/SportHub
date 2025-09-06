using Application.UseCases.Evaluation.AddEvaluation;
using SportHub.Api.Endpoints;

namespace Api.Extensions.Application;

public static class ApplicationExtensions
{
    public static WebApplication UseCustomMiddlewares(this WebApplication app)
    {
        app.UseExceptionHandler();
        app.UseAuthentication();
        app.UseAuthorization();
        
        return app;
    }

    public static WebApplication UseApiEndpoints(this WebApplication app)
    {
        var apiGroup = app.MapGroup("/api/v1")
            .WithOpenApi();

        apiGroup.MapAuthEndpoints();
        apiGroup.MapEstablishmentsEndpoints();
        apiGroup.MapSportsEndpoints();
        apiGroup.MapCourtsEndpoints();
        apiGroup.MapFavoritesEndpoints();
        apiGroup.MapUserEndpoints();
        apiGroup.MapEvaluationEndpoints();

        return app;
    }
}