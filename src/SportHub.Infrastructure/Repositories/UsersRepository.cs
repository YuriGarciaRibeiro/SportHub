using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UsersRepository : BaseRepository<User>, IUsersRepository
{
    private readonly ApplicationDbContext _dbContext;
    public UsersRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbContext.Users
            .Include(u => u.Establishments)
            .FirstOrDefaultAsync(u => u.Email == email);
    }
    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _dbContext.Users.AnyAsync(u => u.Email == email);
    }
}
