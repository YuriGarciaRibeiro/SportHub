using Application.Common.Interfaces;
using Application.UseCases.Establishments.GetEstablishments;
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

    public async Task<(List<EstablishmentResponse> Items, int TotalCount)> GetFilteredAsync(GetEstablishmentsQuery query, CancellationToken cancellationToken)
    {
        var dbQuery = _dbSet
            .Include(e => e.Users)
            .Include(e => e.Sports)
            .AsQueryable();

        if (query.ownerId.HasValue)
        {
            dbQuery = dbQuery.Where(e => e.Users.Any(u => u.UserId == query.ownerId.Value));
        }
        if (query.isAvailable.HasValue)
        {
            dbQuery = dbQuery.Where(e => e.IsDeleted == query.isAvailable.Value);
        }

        var total = await dbQuery.CountAsync(cancellationToken);

        var items = await dbQuery
            .AsNoTracking()
            .OrderBy(e => e.Name)
            .Skip((query.page - 1) * query.pageSize)
            .Take(query.pageSize)
            .Select(e => new EstablishmentResponse(
                e.Id,
                e.Name,
                e.Description,
                new AddressResponse(
                    e.Address.Street,
                    e.Address.Number,
                    e.Address.Complement,
                    e.Address.Neighborhood,
                    e.Address.City,
                    e.Address.State,
                    e.Address.ZipCode
                ),
                e.ImageUrl,
                e.Sports.Select(s => new SportResponse(
                    s.Id,
                    s.Name,
                    s.Description
                ))
            ))
            .ToListAsync(cancellationToken);

        return (items, total);
    }
}
