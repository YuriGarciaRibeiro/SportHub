using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.CQRS;

namespace Application.UseCases.Auth.ResetPassword;

public class ResetPasswordHandler : ICommandHandler<ResetPasswordCommand>
{
    private readonly IUsersRepository _usersRepository;
    private readonly IPasswordService _passwordService;
    private readonly IUnitOfWork _unitOfWork;

    public ResetPasswordHandler(
        IUsersRepository usersRepository,
        IPasswordService passwordService,
        IUnitOfWork unitOfWork)
    {
        _usersRepository = usersRepository;
        _passwordService = passwordService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _usersRepository.GetByPasswordResetTokenAsync(request.Token);
        if (user is null)
            return Result.Fail(new BadRequest("Invalid or expired reset token."));

        if (user.PasswordResetTokenExpiresAt is null || user.PasswordResetTokenExpiresAt < DateTime.UtcNow)
            return Result.Fail(new BadRequest("Reset token has expired."));

        var newHash = _passwordService.HashPassword(request.NewPassword, out var newSalt);
        user.PasswordHash = newHash;
        user.Salt = newSalt;

        // Clear reset token
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiresAt = null;

        // Invalidate refresh tokens to force re-login
        user.RefreshToken = null;
        user.RefreshTokenExpiresAt = null;

        await _usersRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
