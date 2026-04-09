using Application.Common.Interfaces;
using Application.Security;
using Application.UseCases.Tenant.UpdateSettings;
using Application.UseCases.Tenant.UploadTenantCover;
using Application.UseCases.Tenant.UploadTenantLogo;
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

        // POST /api/settings/upload-logo — IsOwner
        app.MapPost("/api/settings/upload-logo", async (
            IFormFile file,
            ISender sender) =>
        {
            if (file is null || file.Length == 0)
                return Results.UnprocessableEntity(new { detail = "Nenhum arquivo enviado." });

            await using var stream = file.OpenReadStream();

            var command = new UploadTenantLogoCommand(
                stream,
                file.FileName,
                file.ContentType,
                file.Length);

            var result = await sender.Send(command);
            return result.ToIResult();
        })
        .WithName("UploadTenantLogo")
        .WithSummary("Faz upload do logo do tenant e atualiza LogoUrl")
        .RequireAuthorization(PolicyNames.IsOwner)
        .DisableAntiforgery()
        .Accepts<IFormFile>("multipart/form-data")
        .WithTags("Settings")
        .Produces<UploadTenantLogoResponse>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status422UnprocessableEntity);

        // POST /api/settings/upload-cover — IsOwner
        app.MapPost("/api/settings/upload-cover", async (
            IFormFile file,
            ISender sender) =>
        {
            if (file is null || file.Length == 0)
                return Results.UnprocessableEntity(new { detail = "Nenhum arquivo enviado." });

            await using var stream = file.OpenReadStream();

            var command = new UploadTenantCoverCommand(
                stream,
                file.FileName,
                file.ContentType,
                file.Length);

            var result = await sender.Send(command);
            return result.ToIResult();
        })
        .WithName("UploadTenantCover")
        .WithSummary("Faz upload da imagem de capa do tenant e atualiza CoverImageUrl")
        .RequireAuthorization(PolicyNames.IsOwner)
        .DisableAntiforgery()
        .Accepts<IFormFile>("multipart/form-data")
        .WithTags("Settings")
        .Produces<UploadTenantCoverResponse>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status422UnprocessableEntity);

        // GET /api/settings/terms — público
        app.MapGet("/api/settings/terms", (ITenantContext tenantCtx) =>
        {
            if (!tenantCtx.IsResolved)
                return Results.NotFound(new { message = "Tenant não encontrado." });

            return Results.Ok(new LegalPageResponse(tenantCtx.TermsOfService));
        })
        .WithName("GetTermsOfService")
        .WithSummary("Retorna os Termos de Serviço personalizados do tenant (público)")
        .AllowAnonymous()
        .WithTags("Settings")
        .Produces<LegalPageResponse>(StatusCodes.Status200OK);

        // GET /api/settings/privacy — público
        app.MapGet("/api/settings/privacy", (ITenantContext tenantCtx) =>
        {
            if (!tenantCtx.IsResolved)
                return Results.NotFound(new { message = "Tenant não encontrado." });

            return Results.Ok(new LegalPageResponse(tenantCtx.PrivacyPolicy));
        })
        .WithName("GetPrivacyPolicy")
        .WithSummary("Retorna a Política de Privacidade personalizada do tenant (público)")
        .AllowAnonymous()
        .WithTags("Settings")
        .Produces<LegalPageResponse>(StatusCodes.Status200OK);

        // PUT /api/settings/legal — IsOwner
        app.MapPut("/api/settings/legal", async (
            UpdateLegalPagesRequest request,
            ITenantContext tenantCtx,
            ITenantRepository tenantRepo,
            IUnitOfWork unitOfWork,
            ICacheService cache) =>
        {
            if (!tenantCtx.IsResolved)
                return Results.NotFound(new { message = "Tenant não encontrado." });

            var tenant = await tenantRepo.GetByIdAsync(tenantCtx.TenantId);
            if (tenant is null)
                return Results.NotFound(new { message = "Tenant não encontrado." });

            tenant.UpdateLegalPages(request.TermsOfService, request.PrivacyPolicy);
            await tenantRepo.UpdateAsync(tenant);
            await unitOfWork.SaveChangesAsync();

            var key = cache.GenerateCacheKey(global::Application.Common.Enums.CacheKeyPrefix.TenantBySlug, tenant.Slug);
            await cache.RemoveAsync(key);

            return Results.NoContent();
        })
        .WithName("UpdateLegalPages")
        .WithSummary("Atualiza os Termos de Serviço e Política de Privacidade do tenant")
        .RequireAuthorization(PolicyNames.IsOwner)
        .WithTags("Settings")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        return app;
    }
}

public record LegalPageResponse(string? Content);
public record UpdateLegalPagesRequest(string? TermsOfService, string? PrivacyPolicy);
