using System.Security.Cryptography;
using Application.Common.Interfaces;
using Application.CQRS;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.Auth.ForgotPassword;

public class ForgotPasswordHandler : ICommandHandler<ForgotPasswordCommand>
{
    private readonly IUsersRepository _usersRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ForgotPasswordHandler> _logger;

    private static readonly TimeSpan TokenExpiry = TimeSpan.FromHours(1);

    public ForgotPasswordHandler(
        IUsersRepository usersRepository,
        IUnitOfWork unitOfWork,
        ILogger<ForgotPasswordHandler> logger)
    {
        _usersRepository = usersRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        // Always return success to avoid email enumeration attacks
        var user = await _usersRepository.GetByEmailAsync(request.Email);
        if (user is null || !user.IsActive)
        {
            _logger.LogInformation("Password reset requested for non-existent or inactive email: {Email}", request.Email);
            return Result.Ok();
        }

        var token = GenerateResetToken();
        user.PasswordResetToken = token;
        user.PasswordResetTokenExpiresAt = DateTime.UtcNow.Add(TokenExpiry);

        await _usersRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // TODO: Replace with email/notification service when available
        _logger.LogWarning("PASSWORD RESET TOKEN for {Email}: {Token} (expires at {ExpiresAt})",
            user.Email, token, user.PasswordResetTokenExpiresAt);

        return Result.Ok();
    }

    private static string GenerateResetToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }
}
