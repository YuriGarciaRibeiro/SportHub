using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CourtsRepository : ICourtsRepository
{
    private readonly ApplicationDbContext _dbContext;

    public CourtsRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Court?> GetByIdAsync(Guid id)
    {
        return await _dbContext.Courts.FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task CreateAsync(Court court)
    {
        await _dbContext.Courts.AddAsync(court);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(Court court)
    {
        _dbContext.Courts.Update(court);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var court = await GetByIdAsync(id);
        if (court != null)
        {
            _dbContext.Courts.Remove(court);
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Court>> GetByEstablishmentIdAsync(Guid establishmentId)
    {
        return await _dbContext.Courts
            .Where(c => c.EstablishmentId == establishmentId)
            .Include(c => c.Sports)
            .ToListAsync();
    }
}
