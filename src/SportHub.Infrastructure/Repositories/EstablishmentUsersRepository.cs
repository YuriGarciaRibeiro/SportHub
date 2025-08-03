using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class EstablishmentUsersRepository : IEstablishmentUsersRepository
{
    private readonly ApplicationDbContext _dbContext;

    public EstablishmentUsersRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(EstablishmentUser establishmentUser)
    {
        await _dbContext.EstablishmentUsers.AddAsync(establishmentUser);
        await _dbContext.SaveChangesAsync();
    }

    public Task AddManyAsync(IEnumerable<EstablishmentUser> establishmentUsers)
    {
        _dbContext.EstablishmentUsers.AddRange(establishmentUsers);
        return _dbContext.SaveChangesAsync();
    }

    public Task<EstablishmentUser?> GetAsync(Guid userId, Guid establishmentId)
    {
        return _dbContext.EstablishmentUsers
            .FirstOrDefaultAsync(eu => eu.UserId == userId && eu.EstablishmentId == establishmentId);
    }

    public Task<List<Guid>> GetByOwnerIdAsync(Guid ownerId)
    {
        return _dbContext.EstablishmentUsers
            .Where(eu => eu.UserId == ownerId && eu.Role == EstablishmentRole.Owner)
            .Select(eu => eu.EstablishmentId)
            .ToListAsync();
    }

    public Task<bool> HasRoleAnywhereAsync(Guid userId, EstablishmentRole requiredRole)
    {
        return _dbContext.EstablishmentUsers
            .AnyAsync(eu => eu.UserId == userId && eu.Role >= requiredRole);
    }

    public Task UpdateAsync(EstablishmentUser establishmentUser)
    {
        _dbContext.EstablishmentUsers.Update(establishmentUser);
        return _dbContext.SaveChangesAsync();
    }
}
