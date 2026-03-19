using Application.CQRS;

namespace Application.UseCases.Sport.UploadSportImage;

public record UploadSportImageCommand(
    Guid SportId,
    Stream FileStream,
    string FileName,
    string ContentType,
    long FileSizeBytes
) : ICommand<UploadSportImageResponse>;
