using Application.Common.Interfaces;
using Application.UserCases.Auth;
using Domain.Enums;
using FluentResults;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IJwtService _jwtService;

    public AuthService(UserManager<AppUser> userManager, IJwtService jwtService)
    {
        _userManager = userManager;
        _jwtService = jwtService;
    }

    public async Task<Result<AuthResponse>> RegisterAsync(string firstName, string lastName, string email, string password)
    {
        var existing = await _userManager.FindByEmailAsync(email);
        if (existing != null)
            return Result.Fail("E-mail is already in use.");

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            Email = email,
            UserName = email,
            FirstName = firstName,
            LastName = lastName
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result.Fail(errors);
        }

        var addRoleResult = await _userManager.AddToRoleAsync(user, UserRole.User.ToString());
        if (!addRoleResult.Succeeded)
        {
            var errors = string.Join(", ", addRoleResult.Errors.Select(e => e.Description));
            return Result.Fail(errors);
        }

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault();

        var (token, expiresAt) = _jwtService.GenerateToken(user.Id, user.FullName, role, user.Email!);
        return Result.Ok(new AuthResponse
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email!,
            Token = token,
            ExpiresAt = expiresAt
        });
    }

    public async Task<Result<AuthResponse>> LoginAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
            return Result.Fail("Invalid credentials.");

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);
        if (!isPasswordValid)
            return Result.Fail("Invalid credentials.");

        // ⬇️ Pegando a role
        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault(); // ou adapte conforme sua lógica

        var (token, expiresAt) = _jwtService.GenerateToken(user.Id, user.FullName, role, user.Email!);

        return Result.Ok(new AuthResponse
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email!,
            Token = token,
            ExpiresAt = expiresAt
        });
    }


}
