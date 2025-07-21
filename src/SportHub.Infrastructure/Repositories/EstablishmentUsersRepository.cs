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

    public Task<EstablishmentUser?> GetAsync(Guid userId, Guid establishmentId)
    {
        return _dbContext.EstablishmentUsers
            .FirstOrDefaultAsync(eu => eu.UserId == userId && eu.EstablishmentId == establishmentId);
    }

    public Task<List<string>> GetByOwnerIdAsync(Guid ownerId)
    {
        return _dbContext.EstablishmentUsers
            .Where(eu => eu.UserId == ownerId && eu.Role == EstablishmentRole.Owner)
            .Select(eu => eu.EstablishmentId.ToString())
            .ToListAsync();
    }



}
