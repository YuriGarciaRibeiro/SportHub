using FluentResults;
using MediatR;
using Application.Common.Interfaces;
using Application.CQRS;

namespace Application.UseCases.Auth.Register;

public class RegisterUserHandler : ICommandHandler<RegisterUserCommand,string>
{
    private readonly IAuthService _authService;

    public RegisterUserHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public Task<Result<string>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        return _authService.RegisterAsync(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Password);
    }
}
