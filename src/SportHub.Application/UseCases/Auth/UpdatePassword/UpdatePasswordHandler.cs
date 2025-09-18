using SportHub.Application.Common.Interfaces.PasswordReset;

namespace Application.UseCases.Auth.UpdatePassword;

public class UpdatePasswordHandler : ICommandHandler<UpdatePasswordCommand>
{
    IPasswordResetService _passwordResetService;

    public UpdatePasswordHandler(IPasswordResetService passwordResetService)
    {
        _passwordResetService = passwordResetService;
    }

    public async Task<Result> Handle(UpdatePasswordCommand request, CancellationToken cancellationToken)
    {
        await _passwordResetService.ResetWithSessionAsync(request.Email, request.SessionId, request.NewPassword);
        return Result.Ok();
    }
}