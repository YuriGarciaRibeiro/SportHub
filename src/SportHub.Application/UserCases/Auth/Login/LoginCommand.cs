using Application.CQRS;
using Application.UserCases.Auth;

public record LoginCommand : ICommand<AuthResponse>
{
    public required string Email { get; init; }
    public required string Password { get; init; }
}
