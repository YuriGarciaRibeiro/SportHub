using Application.CQRS;

namespace Application.UseCases.Auth.Register;

public record RegisterUserCommand: ICommand<AuthResponse>
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }
    public required string Password { get; init; }
}
