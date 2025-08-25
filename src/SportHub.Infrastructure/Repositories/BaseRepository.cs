using Domain.Common;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class BaseRepository<T> : IBaseRepository<T> where T : class, IEntity
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public BaseRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }
    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await _dbSet.FindAsync(id , cancellationToken).ConfigureAwait(false);

    public async Task<T?> GetByIdAsNoTrackingAsync(Guid id, CancellationToken cancellationToken) =>
        await _dbSet.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            .ConfigureAwait(false);

    public async Task<List<T>> GetAllAsync(CancellationToken cancellationToken) =>
        await _dbSet.AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);

    public async Task<List<T>> GetPagedAsync(int skip, int take, CancellationToken cancellationToken) =>
        await _dbSet.AsNoTracking()
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<int> GetCountAsync(CancellationToken cancellationToken) =>
        await _dbSet.CountAsync(cancellationToken).ConfigureAwait(false);

    public async Task AddAsync(T entity, CancellationToken cancellationToken)
    {
        await _dbSet.AddAsync(entity, cancellationToken).ConfigureAwait(false);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateAsync(T entity, CancellationToken cancellationToken)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task RemoveAsync(T entity, CancellationToken cancellationToken)
    {
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<List<T>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
    {
        var idSet = ids.ToHashSet();
        return await _dbSet
            .AsNoTracking()
            .Where(entity => idSet.Contains(entity.Id))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbSet
            .AsNoTracking()
            .AnyAsync(entity => entity.Id == id, cancellationToken)
            .ConfigureAwait(false);
    }

    public IQueryable<T> Query()
    {
        return _dbSet.AsQueryable();
    }

    public IQueryable<T> QueryAsNoTracking()
    {
        return _dbSet.AsNoTracking();
    }

    public async Task AddManyAsync(IEnumerable<T> entities, CancellationToken cancellationToken)
    {
        await _dbSet.AddRangeAsync(entities, cancellationToken).ConfigureAwait(false);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task RemoveByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
    {
        var idSet = ids.ToHashSet();
        await _dbSet
            .Where(entity => idSet.Contains(entity.Id))
            .ExecuteDeleteAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<int> UpdateWhereAsync(
        System.Linq.Expressions.Expression<Func<T, bool>> predicate,
        System.Linq.Expressions.Expression<Func<Microsoft.EntityFrameworkCore.Query.SetPropertyCalls<T>, Microsoft.EntityFrameworkCore.Query.SetPropertyCalls<T>>> setPropertyCalls,
        CancellationToken cancellationToken)
    {
        return await _dbSet
            .Where(predicate)
            .ExecuteUpdateAsync(setPropertyCalls, cancellationToken)
            .ConfigureAwait(false);
    }
}
