using Domain.Entities;

namespace SportHub.Application.Common.Interfaces.PasswordReset;

public interface IOtpCodeRepository
{
    Task RemoveActivesAsync(Guid userId, string purpose, DateTime nowUtc, CancellationToken ct = default);
    Task AddAsync(OtpCode otp, CancellationToken ct = default);
    Task<OtpCode?> GetLatestActiveAsync(Guid userId, string purpose, DateTime nowUtc, CancellationToken ct = default);
    Task UpdateAsync(OtpCode otp, CancellationToken ct = default);
}