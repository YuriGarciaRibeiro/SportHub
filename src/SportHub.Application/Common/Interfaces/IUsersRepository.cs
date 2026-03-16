using Application.Common.Models;
using Domain.Entities;
using Domain.Enums;

namespace Application.Common.Interfaces;

public interface IUsersRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<List<User>> GetAllAsync();
    Task AddAsync(User entity);
    Task UpdateAsync(User entity);
    Task RemoveAsync(User entity);
    Task<List<User>> GetByIdsAsync(IEnumerable<Guid> ids);
    Task<bool> ExistsAsync(Guid id);
    IQueryable<User> Query();
    Task AddManyAsync(IEnumerable<User> entities);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByRefreshTokenAsync(string refreshToken);
    Task<bool> EmailExistsAsync(string email);
    Task<PagedResult<User>> GetPagedAsync(
        int page,
        int pageSize,
        string? email = null,
        string? firstName = null,
        string? lastName = null,
        UserRole? role = null,
        bool? isActive = null,
        string? searchTerm = null,
        IEnumerable<UserRole>? allowedRoles = null);
}
