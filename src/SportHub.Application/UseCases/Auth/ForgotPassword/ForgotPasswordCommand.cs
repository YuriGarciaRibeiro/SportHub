namespace Application.UseCases.Auth.ForgotPassword;

public class ForgotPasswordCommand : ICommand
{
    public string Email { get; set; } = null!;
}