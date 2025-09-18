using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using UserDtos = Application.Common.Interfaces.Users;

namespace Infrastructure.Repositories;

public class UsersRepository : BaseRepository<User>, IUsersRepository
{
    private readonly ApplicationDbContext _dbContext;
    public UsersRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return await _dbContext.Users
            .Where(u => u.Email == email)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);
    }
    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken)
    {
        return await _dbContext.Users.AnyAsync(u => u.Email == email, cancellationToken);
    }

    public async Task UpdatePasswordAsync(User user, string newHash, string newSalt, CancellationToken ct = default)
    {
        user.PasswordHash = newHash;
        user.Salt = newSalt;
        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task IncrementTokenVersionAsync(User user, CancellationToken ct = default)
    {
        user.TokenVersion++;
        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync(ct);
    }
}
