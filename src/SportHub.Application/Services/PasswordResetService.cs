using System.Globalization;
using System.Net;
using Application.Common.Interfaces.Email;
using Application.Common.Interfaces.Users;
using Application.Emails.recoverycode;
using Domain.Entities;
using SportHub.Application.Common.Interfaces.Email;
using SportHub.Application.Common.Interfaces.PasswordReset;
using SportHub.Application.Security;

namespace SportHub.Application.Services;

public class PasswordResetService : IPasswordResetService
{
    private readonly IUsersRepository _users;
    private readonly IOtpCodeRepository _otps;
    private readonly IResetSessionRepository _sessions;
    private readonly ICustomEmailSender _email;
    private readonly IPasswordService _passwordService;
    private readonly IEmailTemplateService _emailTemplateService;

    public PasswordResetService(
        IUsersRepository users,
        IOtpCodeRepository otps,
        IResetSessionRepository sessions,
        ICustomEmailSender email,
        IPasswordService passwordService,
        IEmailTemplateService emailTemplateService)
    {
        _users = users; 
        _otps = otps; 
        _sessions = sessions; 
        _email = email; 
        _passwordService = passwordService;
        _emailTemplateService = emailTemplateService;
    }

    public async Task RequestAsync(string email, CancellationToken ct = default)
    {
        var user = await _users.GetByEmailAsync(email, ct);
        if (user is null) return;

        var now = DateTime.UtcNow;
        await _otps.RemoveActivesAsync(user.Id, "password_reset", now, ct);

        var code = SecurityUtils.GenerateSixDigitCode();
        var codeHash = SecurityUtils.Sha256Hex(code);

        await _otps.AddAsync(new OtpCode
        {
            UserId = user.Id,
            Purpose = "password_reset",
            CodeHash = codeHash,
            ExpiresAt = now.AddMinutes(10)
        }, ct);

        var model = new PasswordResetEmailModel(
            UserName: user.FullName ?? "",
            UserEmail: user.Email,
            VerificationCode: code,
            ExpiryMinutes: 10,
            VerifyUrl: "https://sporthub.app/redefinir",
            SupportEmail: "suporte@sporthub.app",
            SupportUrl: "https://sporthub.app/ajuda",
            Year: DateTime.UtcNow.Year
        );

        var html = await _emailTemplateService.RenderTemplateAsync("recoverycode.html", model, ct);



        await _email.SendAsync(user.Email,
            "Código para redefinir a senha",
            html,
            ct);
    }

    public async Task<bool> VerifyAsync(string email, string code, CancellationToken ct = default)
    {
        var user = await _users.GetByEmailAsync(email, ct);
        if (user is null) return true;

        var now = DateTime.UtcNow;
        var otp = await _otps.GetLatestActiveAsync(user.Id, "password_reset", now, ct);
        if (otp is null || otp.Attempts >= otp.MaxAttempts) return false;

        otp.Attempts++;
        Console.WriteLine(otp.CodeHash);
        Console.WriteLine(SecurityUtils.Sha256Hex(code));
        var ok = otp.CodeHash == SecurityUtils.Sha256Hex(code);
        if (ok) otp.UsedAt = now;

        await _otps.UpdateAsync(otp, ct);

        return ok;
    }

    public async Task<(string SessionId, DateTime ExpiresAt)> CreateSessionAsync(string email, CancellationToken ct = default)
    {
        var user = await _users.GetByEmailAsync(email, ct) ?? throw new UnauthorizedAccessException();

        var sess = new ResetSession
        {
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15)
        };

        await _sessions.AddAsync(sess, ct);
        return (sess.Id, sess.ExpiresAt);
    }

    public async Task ResetWithSessionAsync(string email, string sessionId, string newPassword, bool bumpTokenVersion = true, CancellationToken ct = default)
    {
        var user = await _users.GetByEmailAsync(email, ct);

        if (user is null) throw new UnauthorizedAccessException();

        var now = DateTime.UtcNow;
        var sess = await _sessions.GetActiveAsync(sessionId, user.Id, "password_reset", now, ct)
                   ?? throw new UnauthorizedAccessException("Sessão inválida/expirada.");

        var passwordHash = _passwordService.HashPassword(newPassword, out var salt);
        await _users.UpdatePasswordAsync(user, passwordHash, salt, ct);
        if (bumpTokenVersion) await _users.IncrementTokenVersionAsync(user, ct);

        await _sessions.MarkUsedAsync(sess, now, ct);
    }

    public async Task ResetDirectAsync(string email, string code, string newPassword, bool bumpTokenVersion = true, CancellationToken ct = default)
    {
        var ok = await VerifyAsync(email, code, ct);
        if (!ok) throw new UnauthorizedAccessException("Código inválido ou expirado.");

        var user = await _users.GetByEmailAsync(email, ct) ?? throw new UnauthorizedAccessException();

        var (hash, salt, _) = SecurityUtils.HashPassword(newPassword);
        await _users.UpdatePasswordAsync(user, hash, salt, ct);
        if (bumpTokenVersion) await _users.IncrementTokenVersionAsync(user, ct);
    }

    
}

