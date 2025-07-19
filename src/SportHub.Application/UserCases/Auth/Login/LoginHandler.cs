using Application.Common.Interfaces;
using Application.CQRS;
using Application.UserCases.Auth;

namespace Application.UseCases.Auth.Login;

public class LoginHandler : ICommandHandler<LoginCommand, AuthResponse>
{
    private readonly IAuthService _authService;

    public LoginHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<Result<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        return await _authService.LoginAsync(request.Email, request.Password);
    }
}
