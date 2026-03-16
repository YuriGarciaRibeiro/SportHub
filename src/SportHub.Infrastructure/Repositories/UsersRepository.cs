using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UsersRepository : IUsersRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly DbSet<User> _dbSet;

    public UsersRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
        _dbSet = dbContext.Set<User>();
    }

    public async Task<User?> GetByIdAsync(Guid id) =>
        await _dbSet.FindAsync(id);

    public async Task<List<User>> GetAllAsync() =>
        await _dbSet.ToListAsync();

    public Task AddAsync(User entity)
    {
        _dbSet.Add(entity);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(User entity)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(User entity)
    {
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<List<User>> GetByIdsAsync(IEnumerable<Guid> ids) =>
        await _dbSet.Where(e => ids.Contains(e.Id)).ToListAsync();

    public async Task<bool> ExistsAsync(Guid id) =>
        await _dbSet.AnyAsync(e => e.Id == id);

    public IQueryable<User> Query() =>
        _dbSet.AsQueryable();

    public Task AddManyAsync(IEnumerable<User> entities)
    {
        _dbSet.AddRange(entities);
        return Task.CompletedTask;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByRefreshTokenAsync(string refreshToken)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _dbContext.Users.AnyAsync(u => u.Email == email);
    }

    public async Task<PagedResult<User>> GetPagedAsync(
        int page,
        int pageSize,
        string? email = null,
        string? firstName = null,
        string? lastName = null,
        UserRole? role = null,
        bool? isActive = null,
        string? searchTerm = null,
        IEnumerable<UserRole>? allowedRoles = null)
    {
        var query = _dbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(email))
        {
            query = query.Where(u => u.Email.Contains(email));
        }

        if (!string.IsNullOrWhiteSpace(firstName))
        {
            query = query.Where(u => u.FirstName.Contains(firstName));
        }

        if (!string.IsNullOrWhiteSpace(lastName))
        {
            query = query.Where(u => u.LastName.Contains(lastName));
        }

        if (role.HasValue)
        {
            query = query.Where(u => u.Role == role.Value);
        }

        if (allowedRoles != null)
        {
            var roleList = allowedRoles.ToList();
            query = query.Where(u => roleList.Contains(u.Role));
        }

        if (isActive.HasValue)
        {
            query = query.Where(u => u.IsActive == isActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var search = searchTerm.ToLower();
            query = query.Where(u => 
                u.Email.ToLower().Contains(search) ||
                u.FirstName.ToLower().Contains(search) ||
                u.LastName.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<User>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
