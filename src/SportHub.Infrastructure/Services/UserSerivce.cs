using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using FluentResults;

namespace Infrastructure.Services;

public class UserService : IUserService
{
    private readonly IUsersRepository _usersRepository;

    public UserService(IUsersRepository usersRepository)
    {
        _usersRepository = usersRepository;
    }

    public async Task<Result<User>> GetUserByIdAsync(Guid userId)
    {
        var user = await _usersRepository.GetByIdAsync(userId);

        if (user == null)
            return Result.Fail("User not found.");

        return Result.Ok(user);
    }

    public async Task<Result<List<User>>> GetUsersByIdsAsync(List<Guid> userIds)
    {
        var users = await _usersRepository.GetByIdsAsync(userIds);

        if (users == null || !users.Any())
            return Result.Fail("No users found for the provided IDs.");

        return Result.Ok(users);
    }

    public async Task<Result<User>> AddRoleToUserAsync(Guid userId, UserRole role)
    {
        var user = await _usersRepository.GetByIdAsync(userId);
        if (user is null)
            return Result.Fail("User not found.");

        user.Role = role;
        await _usersRepository.UpdateAsync(user);

        return Result.Ok(user);
    }

    public async Task<Result<User>> RemoveRoleFromUserAsync(Guid userId, UserRole role)
    {
        var user = await _usersRepository.GetByIdAsync(userId);
        if (user is null)
            return Result.Fail("User not found.");

        if (user.Role == role)
        {
            user.Role = UserRole.User;
            await _usersRepository.UpdateAsync(user);
        }

        return Result.Ok(user);
    }

    public async Task<Result<List<User>>> GetByIdsAsync(IEnumerable<Guid> ids)
    {
        var users = await _usersRepository.GetByIdsAsync(ids);
        if (users == null || !users.Any())
            return Result.Fail("No users found for the provided IDs.");

        return Result.Ok(users);
    }
}