using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.CQRS;

namespace Application.UseCases.Auth.RefreshToken;

public class RefreshTokenHandler : ICommandHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly IUsersRepository _usersRepository;
    private readonly IJwtService _jwtService;
    private readonly IUnitOfWork _unitOfWork;

    public RefreshTokenHandler(IUsersRepository usersRepository, IJwtService jwtService, IUnitOfWork unitOfWork)
    {
        _usersRepository = usersRepository;
        _jwtService = jwtService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AuthResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var user = await _usersRepository.GetByRefreshTokenAsync(request.RefreshToken);

        if (user is null || user.IsDeleted || !user.IsActive)
            return Result.Fail(new Unauthorized("Invalid refresh token."));

        if (user.RefreshTokenExpiresAt is null || user.RefreshTokenExpiresAt < DateTime.UtcNow)
            return Result.Fail(new Unauthorized("Refresh token has expired. Please log in again."));

        // Rotate: generate a new access token + a new refresh token
        var (newAccessToken, newAccessExpiresAt) = _jwtService.GenerateToken(
            user.Id,
            user.FullName,
            user.Role.ToString(),
            user.Email
        );

        var (newRefreshToken, newRefreshExpiresAt) = _jwtService.GenerateRefreshToken();

        // Invalidate the old refresh token and save the new one
        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiresAt = newRefreshExpiresAt;
        await _usersRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok(new AuthResponse
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString(),
            Token = newAccessToken,
            ExpiresAt = newAccessExpiresAt,
            RefreshToken = newRefreshToken,
            RefreshTokenExpiresAt = newRefreshExpiresAt
        });
    }
}
