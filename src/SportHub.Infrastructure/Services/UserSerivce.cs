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

    public async Task<Result<User>> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _usersRepository.GetByIdAsync(userId, cancellationToken);

        if (user == null)
            return Result.Fail("User not found.");

        return Result.Ok(user);
    }

    public async Task<Result<List<User>>> GetUsersByIdsAsync(List<Guid> userIds, CancellationToken cancellationToken)
    {
        var users = await _usersRepository.GetByIdsAsync(userIds, cancellationToken);

        if (users == null || !users.Any())
            return Result.Fail("No users found for the provided IDs.");

        return Result.Ok(users);
    }

    public async Task<Result<User>> AddRoleToUserAsync(Guid userId, UserRole role, CancellationToken cancellationToken)
    {
        var user = await _usersRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            return Result.Fail("User not found.");

        user.Role = role;
        await _usersRepository.UpdateAsync(user, cancellationToken);

        return Result.Ok(user);
    }

    public async Task<Result<User>> RemoveRoleFromUserAsync(Guid userId, UserRole role, CancellationToken cancellationToken)
    {
        var user = await _usersRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            return Result.Fail("User not found.");

        if (user.Role == role)
        {
            user.Role = UserRole.User;
            await _usersRepository.UpdateAsync(user, cancellationToken);
        }

        return Result.Ok(user);
    }

    public async Task<Result<List<User>>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
    {
        var users = await _usersRepository.GetByIdsAsync(ids, cancellationToken);
        if (users == null || !users.Any())
            return Result.Fail("No users found for the provided IDs.");

        return Result.Ok(users);
    }
}