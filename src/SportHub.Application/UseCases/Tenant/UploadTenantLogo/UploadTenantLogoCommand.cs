using Application.CQRS;

namespace Application.UseCases.Tenant.UploadTenantLogo;

public record UploadTenantLogoCommand(
    Stream FileStream,
    string FileName,
    string ContentType,
    long FileSizeBytes
) : ICommand<UploadTenantLogoResponse>;
