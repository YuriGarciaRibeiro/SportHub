using Application.Common.Enums;
using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.CQRS;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.Tenant.UploadTenantCover;

public class UploadTenantCoverHandler : ICommandHandler<UploadTenantCoverCommand, UploadTenantCoverResponse>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IStorageService _storageService;
    private readonly ITenantContext _tenantContext;
    private readonly ICacheService _cache;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UploadTenantCoverHandler> _logger;

    public UploadTenantCoverHandler(
        ITenantRepository tenantRepository,
        IStorageService storageService,
        ITenantContext tenantContext,
        ICacheService cache,
        IUnitOfWork unitOfWork,
        ILogger<UploadTenantCoverHandler> logger)
    {
        _tenantRepository = tenantRepository;
        _storageService = storageService;
        _tenantContext = tenantContext;
        _cache = cache;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<UploadTenantCoverResponse>> Handle(
        UploadTenantCoverCommand request,
        CancellationToken cancellationToken)
    {
        if (!_tenantContext.IsResolved)
            return Result.Fail(new Unauthorized("Tenant não resolvido no contexto."));

        var tenant = await _tenantRepository.GetByIdAsync(_tenantContext.TenantId, cancellationToken);
        if (tenant is null)
            return Result.Fail(new NotFound("Tenant não encontrado."));

        var ext = Path.GetExtension(request.FileName).ToLowerInvariant();
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var baseName = Path.GetFileNameWithoutExtension(request.FileName)
            .ToLowerInvariant()
            .Replace(" ", "-");
        var key = $"{_tenantContext.TenantSlug}/cover/{timestamp}-{baseName}{ext}";

        var publicUrl = await _storageService.UploadAsync(
            key,
            request.FileStream,
            request.ContentType,
            cancellationToken);

        _logger.LogInformation(
            "Cover do tenant {Tenant} enviado. Key: {Key}",
            _tenantContext.TenantSlug, key);

        tenant.UpdateCoverImage(publicUrl);
        await _tenantRepository.UpdateAsync(tenant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var slugKey = _cache.GenerateCacheKey(CacheKeyPrefix.TenantBySlug, tenant.Slug);
        var brandingKey = _cache.GenerateCacheKey(CacheKeyPrefix.TenantBranding, tenant.Slug);
        await _cache.RemoveAsync(slugKey, cancellationToken);
        await _cache.RemoveAsync(brandingKey, cancellationToken);

        return Result.Ok(new UploadTenantCoverResponse(publicUrl));
    }
}
