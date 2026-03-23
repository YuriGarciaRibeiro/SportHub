using Application.CQRS;

namespace Application.UseCases.Court.UploadCourtGalleryImage;

public record UploadCourtGalleryImageCommand(
    Guid CourtId,
    Stream FileStream,
    string FileName,
    string ContentType,
    long FileSizeBytes
) : ICommand<UploadCourtGalleryImageResponse>;
