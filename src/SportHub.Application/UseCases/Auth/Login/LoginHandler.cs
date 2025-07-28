using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.CQRS;

namespace Application.UseCases.Auth.Login;

public class LoginHandler : ICommandHandler<LoginCommand, AuthResponse>
{
    private readonly IUsersRepository _usersRepository;
    private readonly IPasswordService _passwordService;
    private readonly IJwtService _jwtService;

    public LoginHandler(
        IUsersRepository usersRepository,
        IPasswordService passwordService,
        IJwtService jwtService)
    {
        _usersRepository = usersRepository;
        _passwordService = passwordService;
        _jwtService = jwtService;
    }

    public async Task<Result<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _usersRepository.GetByEmailAsync(request.Email);
        if (user is null || !user.IsActive)
            return Result.Fail(new Unauthorized("Invalid credentials."));

        if (!_passwordService.VerifyPassword(request.Password, user.PasswordHash, user.Salt))
            return Result.Fail(new Unauthorized("Invalid credentials."));

        if (user.IsDeleted)
            return Result.Fail(new Conflict("User account is deleted."));

        user.LastLoginAt = DateTime.UtcNow;
        await _usersRepository.UpdateAsync(user);

        var (token, expiresAt) = _jwtService.GenerateToken(
            user.Id,
            user.FullName,
            user.Role.ToString(),
            user.Email
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
