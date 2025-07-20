using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class EstablishmentsRepository : IEstablishmentsRepository
{
    private readonly ApplicationDbContext _dbContext;

    public EstablishmentsRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Establishment?> GetByIdAsync(Guid id)
    {
        return await _dbContext.Establishments
            .Include(e => e.Users) 
            .FirstOrDefaultAsync(e => e.Id == id);
    }


    public async Task<List<Establishment>> GetAllAsync()
    {
        return await _dbContext.Establishments
            .Include(e => e.Users)
            .ToListAsync();
    }

    public async Task AddAsync(Establishment establishment)
    {
        await _dbContext.Establishments.AddAsync(establishment);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(Establishment establishment)
    {
        _dbContext.Establishments.Update(establishment);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var establishment = await GetByIdAsync(id);
        if (establishment != null)
        {
            _dbContext.Establishments.Remove(establishment);
            await _dbContext.SaveChangesAsync();
        }
    }
}
