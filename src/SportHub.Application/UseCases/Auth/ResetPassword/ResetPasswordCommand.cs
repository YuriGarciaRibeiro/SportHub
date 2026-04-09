using Application.CQRS;

namespace Application.UseCases.Auth.ResetPassword;

public record ResetPasswordCommand : ICommand
{
    public required string Token { get; init; }
    public required string NewPassword { get; init; }
}
