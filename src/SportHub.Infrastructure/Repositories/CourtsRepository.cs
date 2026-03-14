using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CourtsRepository : BaseRepository<Court>, ICourtsRepository
{
    private readonly ApplicationDbContext _dbContext;

    public CourtsRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public new async Task<List<Court>> GetAllAsync()
    {
        return await _dbContext.Courts
            .Include(c => c.Sports)
            .AsSplitQuery()
            .AsNoTracking()
            .ToListAsync();
    }

    public new async Task<Court?> GetByIdAsync(Guid id)
    {
        return await _dbContext.Courts
            .Include(c => c.Sports)
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.Id == id);
    }
}
