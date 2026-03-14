using Application.CQRS;

namespace Application.UseCases.Auth.RefreshToken;

public class RefreshTokenCommand : ICommand<AuthResponse>
{
    public string RefreshToken { get; set; } = null!;
}
