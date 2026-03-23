using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.CQRS;
using Application.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.UseCases.Court.DeleteCourtGalleryImage;

public class DeleteCourtGalleryImageHandler : ICommandHandler<DeleteCourtGalleryImageCommand, bool>
{
    private readonly ICourtsRepository _courtsRepository;
    private readonly IStorageService _storageService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteCourtGalleryImageHandler> _logger;
    private readonly StorageSettings _storageSettings;

    public DeleteCourtGalleryImageHandler(
        ICourtsRepository courtsRepository,
        IStorageService storageService,
        IUnitOfWork unitOfWork,
        IOptions<StorageSettings> storageOptions,
        ILogger<DeleteCourtGalleryImageHandler> logger)
    {
        _courtsRepository = courtsRepository;
        _storageService = storageService;
        _unitOfWork = unitOfWork;
        _storageSettings = storageOptions.Value;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        DeleteCourtGalleryImageCommand request,
        CancellationToken cancellationToken)
    {
        var court = await _courtsRepository.GetByIdAsync(request.CourtId);
        if (court is null)
            return Result.Fail(new NotFound($"Quadra com ID {request.CourtId} não encontrada."));

        if (!court.ImageUrls.Contains(request.ImageUrl))
            return Result.Fail(new NotFound("Imagem não encontrada na galeria da quadra."));

        var baseUrl = _storageSettings.PublicBaseUrl.TrimEnd('/');
        var key = request.ImageUrl.StartsWith(baseUrl)
            ? request.ImageUrl[(baseUrl.Length + 1)..]
            : request.ImageUrl;

        await _storageService.DeleteAsync(key, cancellationToken);

        _logger.LogInformation(
            "Imagem de galeria removida da quadra {CourtId}. Key: {Key}",
            request.CourtId, key);

        court.ImageUrls.Remove(request.ImageUrl);
        await _courtsRepository.UpdateAsync(court);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok(true);
    }
}
