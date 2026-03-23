using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.CQRS;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.Court.UploadCourtGalleryImage;

public record UploadCourtGalleryImageResponse(string ImageUrl);

public class UploadCourtGalleryImageHandler : ICommandHandler<UploadCourtGalleryImageCommand, UploadCourtGalleryImageResponse>
{
    private readonly ICourtsRepository _courtsRepository;
    private readonly IStorageService _storageService;
    private readonly ITenantContext _tenantContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UploadCourtGalleryImageHandler> _logger;

    public UploadCourtGalleryImageHandler(
        ICourtsRepository courtsRepository,
        IStorageService storageService,
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork,
        ILogger<UploadCourtGalleryImageHandler> logger)
    {
        _courtsRepository = courtsRepository;
        _storageService = storageService;
        _tenantContext = tenantContext;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<UploadCourtGalleryImageResponse>> Handle(
        UploadCourtGalleryImageCommand request,
        CancellationToken cancellationToken)
    {
        var court = await _courtsRepository.GetByIdAsync(request.CourtId);
        if (court is null)
            return Result.Fail(new NotFound($"Quadra com ID {request.CourtId} não encontrada."));

        var ext = Path.GetExtension(request.FileName).ToLowerInvariant();
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var baseName = Path.GetFileNameWithoutExtension(request.FileName)
            .ToLowerInvariant()
            .Replace(" ", "-");
        var key = $"{_tenantContext.TenantSlug}/courts/{request.CourtId}/gallery/{timestamp}-{baseName}{ext}";

        var publicUrl = await _storageService.UploadAsync(
            key,
            request.FileStream,
            request.ContentType,
            cancellationToken);

        _logger.LogInformation(
            "Imagem de galeria da quadra {CourtId} (tenant {Tenant}) enviada. Key: {Key}",
            request.CourtId, _tenantContext.TenantSlug, key);

        court.ImageUrls.Add(publicUrl);
        await _courtsRepository.UpdateAsync(court);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok(new UploadCourtGalleryImageResponse(publicUrl));
    }
}
