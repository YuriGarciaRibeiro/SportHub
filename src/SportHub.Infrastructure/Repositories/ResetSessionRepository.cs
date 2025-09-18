using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SportHub.Application.Common.Interfaces.PasswordReset;

namespace SportHub.Infrastructure.Repositories;

// Infrastructure/Persistence/Repositories/ResetSessionRepository.cs
public class ResetSessionRepository : IResetSessionRepository
{
    private readonly ApplicationDbContext _db;
    public ResetSessionRepository(ApplicationDbContext db) => _db = db;

    public async Task AddAsync(ResetSession session, CancellationToken ct = default)
    {
        _db.ResetSessions.Add(session);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<ResetSession?> GetActiveAsync(string sessionId, Guid userId, string purpose, DateTime nowUtc, CancellationToken ct = default) =>
        await _db.ResetSessions.FirstOrDefaultAsync(s =>
            s.Id == sessionId && s.UserId == userId && s.Purpose == purpose && s.UsedAt == null && s.ExpiresAt > nowUtc, ct);

    public async Task MarkUsedAsync(ResetSession session, DateTime usedAtUtc, CancellationToken ct = default)
    {
        session.UsedAt = usedAtUtc;
        _db.ResetSessions.Update(session);
        await _db.SaveChangesAsync(ct);
    }
}
