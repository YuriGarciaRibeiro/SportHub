using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.CQRS;
using Domain.Entities;
using Domain.Enums;

namespace Application.UseCases.Auth.Register;

public class RegisterUserHandler : ICommandHandler<RegisterUserCommand, AuthResponse>
{
    private readonly IUsersRepository _usersRepository;
    private readonly IPasswordService _passwordService;
    private readonly IJwtService _jwtService;

    public RegisterUserHandler(
        IUsersRepository usersRepository,
        IPasswordService passwordService,
        IJwtService jwtService)
    {
        _usersRepository = usersRepository;
        _passwordService = passwordService;
        _jwtService = jwtService;
    }

    public async Task<Result<AuthResponse>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        if (await _usersRepository.EmailExistsAsync(request.Email))
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

        await _usersRepository.AddAsync(user);

        var (token, expiresAt) = _jwtService.GenerateToken(
            user.Id, user.FullName, user.Role.ToString(), user.Email);

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
