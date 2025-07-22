using Application.Common.Interfaces;
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
        // Verificar se o email já existe
        if (await _usersRepository.EmailExistsAsync(email))
            return Result.Fail("E-mail is already in use.");

        // Validar senha (você pode implementar validações personalizadas aqui)
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            return Result.Fail("Password must be at least 8 characters long.");

        // Hash da senha
        var passwordHash = _passwordService.HashPassword(password, out var salt);

        // Criar novo usuário
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

            // Gerar token JWT
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
        catch (Exception ex)
        {
            return Result.Fail($"Error creating user: {ex.Message}");
        }
    }

    public async Task<Result<AuthResponse>> LoginAsync(string email, string password)
    {
        var user = await _usersRepository.GetByEmailAsync(email);
        if (user is null || !user.IsActive)
            return Result.Fail("Invalid credentials.");

        // Verificar senha
        if (!_passwordService.VerifyPassword(password, user.PasswordHash, user.Salt))
            return Result.Fail("Invalid credentials.");

        // Atualizar último login
        user.LastLoginAt = DateTime.UtcNow;
        await _usersRepository.UpdateAsync(user);

        // Gerar token JWT
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
