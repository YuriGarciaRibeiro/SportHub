using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.CQRS;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.Sport.UploadSportImage;

public class UploadSportImageHandler : ICommandHandler<UploadSportImageCommand, UploadSportImageResponse>
{
    private readonly ISportsRepository _sportsRepository;
    private readonly IStorageService _storageService;
    private readonly ITenantContext _tenantContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UploadSportImageHandler> _logger;

    public UploadSportImageHandler(
        ISportsRepository sportsRepository,
        IStorageService storageService,
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork,
        ILogger<UploadSportImageHandler> logger)
    {
        _sportsRepository = sportsRepository;
        _storageService = storageService;
        _tenantContext = tenantContext;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<UploadSportImageResponse>> Handle(
        UploadSportImageCommand request,
        CancellationToken cancellationToken)
    {
        var sport = await _sportsRepository.GetByIdAsync(request.SportId);
        if (sport is null)
            return Result.Fail(new NotFound($"Esporte com ID {request.SportId} não encontrado."));

        var ext = Path.GetExtension(request.FileName).ToLowerInvariant();
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var baseName = Path.GetFileNameWithoutExtension(request.FileName)
            .ToLowerInvariant()
            .Replace(" ", "-");
        var key = $"{_tenantContext.TenantSlug}/sports/{request.SportId}/{timestamp}-{baseName}{ext}";

        var publicUrl = await _storageService.UploadAsync(
            key,
            request.FileStream,
            request.ContentType,
            cancellationToken);

        _logger.LogInformation(
            "Imagem do esporte {SportId} (tenant {Tenant}) enviada. Key: {Key}",
            request.SportId, _tenantContext.TenantSlug, key);

        sport.ImageUrl = publicUrl;
        await _sportsRepository.UpdateAsync(sport);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok(new UploadSportImageResponse(publicUrl));
    }
}
