namespace Application.UseCases.Auth.VerifyForgotPasswordCode;

public class VerifyForgotPasswordCodeResponse
{
    public string SessionId { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
}