using Application.CQRS;

namespace Application.UseCases.Auth.ChangePassword;

public record ChangePasswordCommand : ICommand
{
    public required string CurrentPassword { get; init; }
    public required string NewPassword { get; init; }
}
