using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.CQRS;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.Auth.Login;

public class LoginHandler : ICommandHandler<LoginCommand, AuthResponse>
{
    private readonly IUsersRepository _usersRepository;
    private readonly IPasswordService _passwordService;
    private readonly IJwtService _jwtService;
    private readonly ILogger<LoginHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public LoginHandler(
        IUsersRepository usersRepository,
        IPasswordService passwordService,
        IJwtService jwtService,
        ILogger<LoginHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _usersRepository = usersRepository;
        _passwordService = passwordService;
        _jwtService = jwtService;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _usersRepository.GetByEmailAsync(request.Email);
            if (user is null || !user.IsActive)
                return Result.Fail(new Unauthorized("Invalid credentials."));

            if (!_passwordService.VerifyPassword(request.Password, user.PasswordHash, user.Salt))
                return Result.Fail(new Unauthorized("Invalid credentials."));

            if (user.IsDeleted)
                return Result.Fail(new Conflict("User account is deleted."));

            var fullName = $"{user.FirstName} {user.LastName}".Trim();

            var (accessToken, accessExpiresAt) = _jwtService.GenerateToken(
                user.Id,
                fullName,
                user.Role.ToString(),
                user.Email
            );

            var (refreshToken, refreshExpiresAt) = _jwtService.GenerateRefreshToken();

            user.LastLoginAt = DateTime.UtcNow;
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiresAt = refreshExpiresAt;
            await _usersRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Ok(new AuthResponse
            {
                UserId = user.Id,
                FullName = fullName,
                Email = user.Email,
                Role = user.Role.ToString(),
                Token = accessToken,
                ExpiresAt = accessExpiresAt,
                RefreshToken = refreshToken,
                RefreshTokenExpiresAt = refreshExpiresAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "REAL LOGIN EXCEPTION for {Email}: {Msg}", request.Email, ex.Message);
            return Result.Fail(new Unauthorized("Invalid credentials."));
        }
    }
}
