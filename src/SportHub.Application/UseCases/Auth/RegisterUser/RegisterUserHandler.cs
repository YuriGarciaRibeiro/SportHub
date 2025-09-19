using Application.Common.Errors;
using Application.CQRS;
using Domain.Entities;
using Domain.Enums;
using Application.Common.Interfaces.Email;
using SportHub.Application.Common.Interfaces.Email;
using Application.Emails.welcome;

namespace Application.UseCases.Auth.Register;

public class RegisterUserHandler : ICommandHandler<RegisterUserCommand, AuthResponse>
{
    private readonly IUserService _userService;
    private readonly IPasswordService _passwordService;
    private readonly IJwtService _jwtService;
    private readonly IEmailTemplateService _emailTemplateService;
    private readonly ICustomEmailSender _emailSender;

    public RegisterUserHandler(
        IUserService userService,
        IPasswordService passwordService,
        IJwtService jwtService,
        IEmailTemplateService emailTemplateService,
        ICustomEmailSender emailSender)
    {
        _userService = userService;
        _passwordService = passwordService;
        _jwtService = jwtService;
        _emailTemplateService = emailTemplateService;
        _emailSender = emailSender;
    }

    public async Task<Result<AuthResponse>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        if (await _userService.EmailExistsAsync(request.Email, cancellationToken))
            return Result.Fail(new Conflict($"E-mail '{request.Email}' is already in use."));

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
            return Result.Fail(new BadRequest("Password must be at least 8 characters long."));

        var passwordHash = _passwordService.HashPassword(request.Password, out var salt);

        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PasswordHash = passwordHash,
            Salt = salt,
            Role = UserRole.User,
            IsActive = true
        };

        await _userService.CreateAsync(user, cancellationToken);

        await SendWelcomeEmailAsync(user, cancellationToken);

        var (token, expiresAt) = _jwtService.GenerateToken(
            user.Id, user.FullName, user.Role.ToString(), user.Email, user.TokenVersion.ToString());

        return Result.Ok(new AuthResponse
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Token = token,
            ExpiresAt = expiresAt
        });
    }

    private async Task SendWelcomeEmailAsync(User user, CancellationToken cancellationToken)
    {
        try
        {
            var model = new WelcomeEmailModel(
                UserName: user.FullName,
                UserEmail: user.Email,
                AppUrl: "https://sporthub.app",
                VerifyEmailUrl: "https://sporthub.app/verificar-email",
                SupportEmail: "suporte@sporthub.app",
                SupportUrl: "https://sporthub.app/ajuda",
                Year: DateTime.UtcNow.Year
            );

            var html = await _emailTemplateService.RenderTemplateAsync("welcome.html", model, cancellationToken);

            await _emailSender.SendAsync(
                user.Email,
                "Bem-vindo ao SportHub! 🏆",
                html,
                cancellationToken);
        }
        catch (Exception)
        {
        }
    }

    
}
