
using Application.Common.Errors;
using SportHub.Application.Common.Interfaces.PasswordReset;

namespace Application.UseCases.Auth.VerifyForgotPasswordCode;

public class VerifyForgotPasswordCodeHandler : ICommandHandler<VerifyForgotPasswordCodeCommand, VerifyForgotPasswordCodeResponse>
{

    private readonly IPasswordResetService _passwordResetService;

    public VerifyForgotPasswordCodeHandler(IPasswordResetService passwordResetService)
    {
        _passwordResetService = passwordResetService;
    }

    public async Task<Result<VerifyForgotPasswordCodeResponse>> Handle(VerifyForgotPasswordCodeCommand request, CancellationToken cancellationToken)
    {
        var isCodeValid = await _passwordResetService.VerifyAsync(request.Email, request.Code, cancellationToken);

        if (!isCodeValid) return Result.Fail(new Unauthorized(""));

        var (SessionId, ExpiresAt) = await _passwordResetService.CreateSessionAsync(request.Email, cancellationToken);

        var response = new VerifyForgotPasswordCodeResponse
        {
            SessionId = SessionId,
            ExpiresAt = ExpiresAt
        };

        return response;
    }
}