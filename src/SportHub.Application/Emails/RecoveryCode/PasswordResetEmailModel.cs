namespace Application.Emails.recoverycode;

public sealed record PasswordResetEmailModel(
        string UserName,
        string UserEmail,
        string VerificationCode,
        int    ExpiryMinutes,
        string? VerifyUrl,
        string? SupportEmail,
        string? SupportUrl,
        int    Year);