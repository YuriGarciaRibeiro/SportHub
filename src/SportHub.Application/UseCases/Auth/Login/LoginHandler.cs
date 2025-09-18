using Application.Common.Errors;
using Application.CQRS;

namespace Application.UseCases.Auth.Login;

public class LoginHandler : ICommandHandler<LoginCommand, AuthResponse>
{
    private readonly IUserService _userService;
    private readonly IPasswordService _passwordService;
    private readonly IJwtService _jwtService;

    public LoginHandler(
        IUserService userService,
        IPasswordService passwordService,
        IJwtService jwtService)
    {
        _userService = userService;
        _passwordService = passwordService;
        _jwtService = jwtService;
    }

    public async Task<Result<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userService.GetUserByEmailAsync(request.Email, cancellationToken);
        if (user is null || !user.IsActive)
            return Result.Fail(new Unauthorized("Invalid credentials."));

        if (!_passwordService.VerifyPassword(request.Password, user.PasswordHash, user.Salt))
            return Result.Fail(new Unauthorized("Invalid credentials."));

        if (user.IsDeleted)
            return Result.Fail(new Conflict("User account is deleted."));

        user.LastLoginAt = DateTime.UtcNow;
        await _userService.UpdateAsync(user, cancellationToken);

        var (token, expiresAt) = _jwtService.GenerateToken(
            user.Id,
            user.FullName,
            user.Role.ToString(),
            user.Email,
            user.TokenVersion.ToString()
        );

        return Result.Ok(new AuthResponse
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Token = token,
            ExpiresAt = expiresAt
        });
    }
}
