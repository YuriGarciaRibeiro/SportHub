

using SportHub.Application.Common.Interfaces.PasswordReset;

namespace Application.UseCases.Auth.ForgotPassword;

public class ForgotPasswordHandler : ICommandHandler<ForgotPasswordCommand>
{

    IPasswordResetService _passwordResetService;

    public ForgotPasswordHandler(IPasswordResetService passwordResetService)
    {
        _passwordResetService = passwordResetService;
    }

    public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        await _passwordResetService.RequestAsync(request.Email, cancellationToken);
        return Result.Ok();
    }
}