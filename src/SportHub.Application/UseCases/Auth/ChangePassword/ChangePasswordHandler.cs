using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.CQRS;

namespace Application.UseCases.Auth.ChangePassword;

public class ChangePasswordHandler : ICommandHandler<ChangePasswordCommand>
{
    private readonly IUsersRepository _usersRepository;
    private readonly IPasswordService _passwordService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public ChangePasswordHandler(
        IUsersRepository usersRepository,
        IPasswordService passwordService,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _usersRepository = usersRepository;
        _passwordService = passwordService;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _usersRepository.GetByIdAsync(_currentUserService.UserId);
        if (user is null)
            return Result.Fail(new NotFound("User not found."));

        if (!_passwordService.VerifyPassword(request.CurrentPassword, user.PasswordHash, user.Salt))
            return Result.Fail(new BadRequest("Current password is incorrect."));

        var newHash = _passwordService.HashPassword(request.NewPassword, out var newSalt);
        user.PasswordHash = newHash;
        user.Salt = newSalt;

        // Invalidate refresh token to force re-login on other devices
        user.RefreshToken = null;
        user.RefreshTokenExpiresAt = null;

        await _usersRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
