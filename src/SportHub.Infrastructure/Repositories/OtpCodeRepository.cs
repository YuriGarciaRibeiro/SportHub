using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SportHub.Application.Common.Interfaces.PasswordReset;

namespace SportHub.Infrastructure.Repositories;

public class OtpCodeRepository : IOtpCodeRepository
{
    private readonly ApplicationDbContext _db;
    public OtpCodeRepository(ApplicationDbContext db) => _db = db;

    public async Task RemoveActivesAsync(Guid userId, string purpose, DateTime nowUtc, CancellationToken ct = default)
    {
        var q = _db.OtpCodes.Where(o => o.UserId == userId && o.Purpose == purpose && o.UsedAt == null && o.ExpiresAt > nowUtc);
        _db.OtpCodes.RemoveRange(q);
        await _db.SaveChangesAsync();
    }

    public async Task AddAsync(OtpCode otp, CancellationToken ct = default)
    {
        _db.OtpCodes.Add(otp);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<OtpCode?> GetLatestActiveAsync(Guid userId, string purpose, DateTime nowUtc, CancellationToken ct = default) =>
        await _db.OtpCodes
           .Where(o => o.UserId == userId && o.Purpose == purpose && o.UsedAt == null && o.ExpiresAt > nowUtc)
           .OrderByDescending(o => o.CreatedAt)
           .FirstOrDefaultAsync(ct);

    public async Task UpdateAsync(OtpCode otp, CancellationToken ct = default)
    {
        _db.OtpCodes.Update(otp);
        await _db.SaveChangesAsync(ct);
    }
}
