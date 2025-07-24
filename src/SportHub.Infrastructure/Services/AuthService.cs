using Application.Common.Interfaces;
using Application.Common.Errors;
using Application.UserCases.Auth;
using Domain.Entities;
using Domain.Enums;
using FluentResults;

namespace Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUsersRepository _usersRepository;
    private readonly IPasswordService _passwordService;
    private readonly IJwtService _jwtService;

    public AuthService(
        IUsersRepository usersRepository,
        IPasswordService passwordService,
        IJwtService jwtService)
    {
        _usersRepository = usersRepository;
        _passwordService = passwordService;
        _jwtService = jwtService;
    }

    public async Task<Result<AuthResponse>> RegisterAsync(string firstName, string lastName, string email, string password)
    {
        if (await _usersRepository.EmailExistsAsync(email))
            return Result.Fail(new Conflict($"E-mail '{email}' is already in use."));

        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            return Result.Fail(new BadRequest("Password must be at least 8 characters long."));

        var passwordHash = _passwordService.HashPassword(password, out var salt);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            PasswordHash = passwordHash,
            Salt = salt,
            Role = UserRole.User,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            await _usersRepository.CreateAsync(user);

            var (token, expiresAt) = _jwtService.GenerateToken(
                user.Id, 
                user.FullName, 
                user.Role.ToString(), 
                user.Email);

            return Result.Ok(new AuthResponse
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Token = token,
                ExpiresAt = expiresAt
            });
        }
        catch (Exception)
        {
            return Result.Fail(new InternalServerError("Error creating user."));
        }
    }

    public async Task<Result<AuthResponse>> LoginAsync(string email, string password)
    {
        var user = await _usersRepository.GetByEmailAsync(email);
        if (user is null || !user.IsActive)
            return Result.Fail(new Unauthorized("Invalid credentials."));

        if (!_passwordService.VerifyPassword(password, user.PasswordHash, user.Salt))
            return Result.Fail(new Unauthorized("Invalid credentials."));

        user.LastLoginAt = DateTime.UtcNow;
        await _usersRepository.UpdateAsync(user);

        var (token, expiresAt) = _jwtService.GenerateToken(
            user.Id, 
            user.FullName, 
            user.Role.ToString(), 
            user.Email);

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
