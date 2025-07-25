using Application.CQRS;

namespace Application.UseCases.Auth.Login;

public record LoginCommand : ICommand<AuthResponse>
{
    public required string Email { get; init; }
    public required string Password { get; init; }
}
