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

    public async Task AddAsync(EstablishmentUser establishmentUser, CancellationToken cancellationToken)
    {
        await _dbContext.EstablishmentUsers.AddAsync(establishmentUser, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task AddManyAsync(IEnumerable<EstablishmentUser> establishmentUsers, CancellationToken cancellationToken)
    {
        _dbContext.EstablishmentUsers.AddRange(establishmentUsers);
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<EstablishmentUser?> GetAsync(Guid userId, Guid establishmentId, CancellationToken cancellationToken)
    {
        return _dbContext.EstablishmentUsers
            .FirstOrDefaultAsync(eu => eu.UserId == userId && eu.EstablishmentId == establishmentId, cancellationToken);
    }

    public Task<List<Guid>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken)
    {
        return _dbContext.EstablishmentUsers
            .Where(eu => eu.UserId == ownerId && eu.Role == EstablishmentRole.Owner)
            .Select(eu => eu.EstablishmentId)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> HasRoleAnywhereAsync(Guid userId, EstablishmentRole requiredRole, CancellationToken cancellationToken)
    {
        return _dbContext.EstablishmentUsers
            .AnyAsync(eu => eu.UserId == userId && eu.Role >= requiredRole, cancellationToken);
    }

    public Task UpdateAsync(EstablishmentUser establishmentUser, CancellationToken cancellationToken)
    {
        _dbContext.EstablishmentUsers.Update(establishmentUser);
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
