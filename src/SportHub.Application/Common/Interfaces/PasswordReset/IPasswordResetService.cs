namespace SportHub.Application.Common.Interfaces.PasswordReset;

public interface IPasswordResetService
{
    Task RequestAsync(string email, CancellationToken ct = default);
    Task<bool> VerifyAsync(string email, string code, CancellationToken ct = default);
    Task<(string SessionId, DateTime ExpiresAt)> CreateSessionAsync(string email, CancellationToken ct = default);
    Task ResetWithSessionAsync(string email, string sessionId, string newPassword, bool bumpTokenVersion = true, CancellationToken ct = default);
    Task ResetDirectAsync(string email, string code, string newPassword, bool bumpTokenVersion = true, CancellationToken ct = default);
}