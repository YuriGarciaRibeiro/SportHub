using Application.CQRS;

namespace Application.UseCases.Court.UploadCourtImage;

public record UploadCourtImageCommand(
    Guid CourtId,
    Stream FileStream,
    string FileName,
    string ContentType,
    long FileSizeBytes
) : ICommand<UploadCourtImageResponse>;
