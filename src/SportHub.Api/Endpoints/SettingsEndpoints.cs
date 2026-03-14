using Application.Security;
using Application.UseCases.Tenant.UpdateSettings;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebApi.Extensions.ResultExtensions;

namespace SportHub.Api.Endpoints;

public static class SettingsEndpoints
{
    public static IEndpointRouteBuilder MapSettingsEndpoints(this IEndpointRouteBuilder app)
    {
        // PUT /api/settings — IsOwner
        app.MapPut("/api/settings", async (UpdateSettingsCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.ToIResult();
        })
        .WithName("UpdateSettings")
        .WithSummary("Atualiza as configurações (nome, logo, cor) do tenant atual")
        .RequireAuthorization(PolicyNames.IsOwner)
        .WithTags("Settings")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        return app;
    }
}
