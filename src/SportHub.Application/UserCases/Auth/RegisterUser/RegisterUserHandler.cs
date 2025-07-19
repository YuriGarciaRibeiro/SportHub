using FluentResults;
using MediatR;
using Application.Common.Interfaces;
using Application.CQRS;
using Application.UserCases.Auth;

namespace Application.UseCases.Auth.Register;

public class RegisterUserHandler : ICommandHandler<RegisterUserCommand, AuthResponse>
{
    private readonly IAuthService _authService;

    public RegisterUserHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public Task<Result<AuthResponse>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        return _authService.RegisterAsync(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Password);
    }
}
