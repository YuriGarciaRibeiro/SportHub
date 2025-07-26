using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

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
            .Include(e => e.Courts)
            .Include(e => e.Sports)
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

    public Task<List<Establishment>> GetByIdsAsync(IEnumerable<Guid> ids)
    {   
        return _dbContext.Establishments
            .Where(e => ids.Contains(e.Id))
            .Include(e => e.Users)
            .Include(e => e.Courts)
                .ThenInclude(c => c.Sports)
            .Include(e => e.Sports)
            .ToListAsync();
    }
}
