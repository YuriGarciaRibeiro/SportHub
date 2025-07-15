using Application.CQRS;

namespace Application.UseCases.Auth.Register;

public record RegisterUserCommand(string FullName, string Email, string Password) : ICommand<string>
{
    public string FullName { get; init; } = FullName;
    public string Email { get; init; } = Email;
    public string Password { get; init; } = Password;
}
