namespace Application.UseCases.Auth.UpdatePassword;

public class UpdatePasswordCommand : ICommand
{
    public string SessionId { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string NewPassword { get; set; } = null!;
}