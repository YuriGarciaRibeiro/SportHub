using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class EstablishmentsRepository : BaseRepository<Establishment>, IEstablishmentsRepository
{
    private readonly ApplicationDbContext _dbContext;
    public EstablishmentsRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Establishment>> GetByIdsWithDetailsAsync(IEnumerable<Guid> ids)
    {
        return await _dbContext.Establishments
            .Where(e => ids.Contains(e.Id))
            .Include(e => e.Courts)
                .ThenInclude(c => c.Sports)
            .Include(e => e.Users)
            .Include(e => e.Sports)
            .AsSplitQuery()
            .AsNoTracking()
            .ToListAsync();
    }
}
