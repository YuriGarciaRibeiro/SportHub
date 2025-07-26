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

    public async Task<IEnumerable<Court>> GetByEstablishmentIdAsync(Guid establishmentId)
    {
        return await _dbContext.Courts
            .Where(c => c.EstablishmentId == establishmentId)
            .Include(c => c.Sports)
            .AsSplitQuery()
            .AsNoTracking()
            .ToListAsync();
    }
}
