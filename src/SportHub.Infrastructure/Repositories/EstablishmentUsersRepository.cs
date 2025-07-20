using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Identity;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

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

    public Task<List<EstablishmentUser>> GetByEstablishmentIdAsync(Guid establishmentId)
    {
        throw new NotImplementedException();
    }

    public Task<EstablishmentUser?> GetByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<List<string>> GetByOwnerIdAsync(Guid ownerId)
    {
        return _dbContext.EstablishmentUsers
            .Where(eu => eu.UserId == ownerId && eu.Role == EstablishmentRole.Owner)
            .Select(eu => eu.EstablishmentId.ToString())
            .ToListAsync();
    }

    public Task<List<EstablishmentUser>> GetByUserIdAsync(Guid userId)
    {
        throw new NotImplementedException();
    }

    
}
