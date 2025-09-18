using Domain.Entities;

namespace SportHub.Application.Common.Interfaces.PasswordReset;

public interface IResetSessionRepository
{
    Task AddAsync(ResetSession session, CancellationToken ct = default);
    Task<ResetSession?> GetActiveAsync(string sessionId, Guid userId, string purpose, DateTime nowUtc, CancellationToken ct = default);
    Task MarkUsedAsync(ResetSession session, DateTime usedAtUtc, CancellationToken ct = default);
}