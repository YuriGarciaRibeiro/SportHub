namespace Application.UseCases.Auth.VerifyForgotPasswordCode;

public class VerifyForgotPasswordCodeCommand : ICommand<VerifyForgotPasswordCodeResponse>
{
    public string Email { get; set; } = null!;
    public string Code { get; set; } = null!;
}