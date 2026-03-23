using Application.CQRS;

namespace Application.UseCases.Tenant.UploadTenantCover;

public record UploadTenantCoverCommand(
    Stream FileStream,
    string FileName,
    string ContentType,
    long FileSizeBytes
) : ICommand<UploadTenantCoverResponse>;
