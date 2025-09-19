using System.Globalization;
using System.Net;
using System.Reflection;
using Application.Common.Interfaces.Email;
using Application.Common.Interfaces.Users;
using Domain.Entities;
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

    public PasswordResetService(
        IUsersRepository users,
        IOtpCodeRepository otps,
        IResetSessionRepository sessions,
        ICustomEmailSender email,
        IPasswordService passwordService)
    {
        _users = users; _otps = otps; _sessions = sessions; _email = email; _passwordService = passwordService;
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

        var html = await GetEmailTemplateAsync();

        // Preenche placeholders
        html = ApplyEmailTemplate(html, new PasswordResetEmailModel(
            UserName: user.FullName ?? "",
            UserEmail: user.Email,
            VerificationCode: code,
            ExpiryMinutes: 10,
            VerifyUrl: "https://sporthub.app/redefinir",   // ajuste se quiser
            SupportEmail: "suporte@sporthub.app",
            SupportUrl: "https://sporthub.app/ajuda",
            Year: DateTime.UtcNow.Year
        ));



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

    private static async Task<string> GetEmailTemplateAsync()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "SportHub.Application.Emails.recoverycode.html";
        
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new FileNotFoundException($"Embedded resource '{resourceName}' not found.");
        }
        
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }

    private static string ApplyEmailTemplate(string html, PasswordResetEmailModel m)
    {
        // Se seu HTML usa os 6 dígitos separadamente: {{VerificationCode:0}} ... :5
        // Garante 6 chars
        var code = (m.VerificationCode ?? "").PadLeft(6, '0');
        html = html
            .Replace("{{VerificationCode}}", code)
            .Replace("{{VerificationCode:0}}", code[0].ToString())
            .Replace("{{VerificationCode:1}}", code[1].ToString())
            .Replace("{{VerificationCode:2}}", code[2].ToString())
            .Replace("{{VerificationCode:3}}", code[3].ToString())
            .Replace("{{VerificationCode:4}}", code[4].ToString())
            .Replace("{{VerificationCode:5}}", code[5].ToString());

        // Demais placeholders (encode onde for texto)
        string H(string s) => WebUtility.HtmlEncode(s ?? "");

        return html
            .Replace("{{UserName}}", H(m.UserName))
            .Replace("{{UserEmail}}", H(m.UserEmail))
            .Replace("{{ExpiryMinutes}}", m.ExpiryMinutes.ToString(CultureInfo.InvariantCulture))
            .Replace("{{VerifyUrl}}", m.VerifyUrl ?? "")
            .Replace("{{SupportEmail}}", H(m.SupportEmail ?? ""))
            .Replace("{{SupportUrl}}", m.SupportUrl ?? "")
            .Replace("{{Year}}", m.Year.ToString());
    }

    private sealed record PasswordResetEmailModel(
        string UserName,
        string UserEmail,
        string VerificationCode,
        int    ExpiryMinutes,
        string? VerifyUrl,
        string? SupportEmail,
        string? SupportUrl,
        int    Year);
}

