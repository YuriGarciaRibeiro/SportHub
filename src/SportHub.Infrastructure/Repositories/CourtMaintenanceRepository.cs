using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CourtMaintenanceRepository : ICourtMaintenanceRepository
{
    private readonly ApplicationDbContext _dbContext;

    public CourtMaintenanceRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CourtMaintenance?> GetByIdAsync(Guid id)
    {
        return await _dbContext.CourtMaintenances
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<List<CourtMaintenance>> GetByCourtIdAsync(Guid courtId)
    {
        return await _dbContext.CourtMaintenances
            .AsNoTracking()
            .Where(m => m.CourtId == courtId)
            .OrderBy(m => m.Type)
            .ThenBy(m => m.DayOfWeek)
            .ThenBy(m => m.Date)
            .ToListAsync();
    }

    public Task AddAsync(CourtMaintenance entity)
    {
        _dbContext.CourtMaintenances.Add(entity);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(CourtMaintenance entity)
    {
        _dbContext.CourtMaintenances.Remove(entity);
        return Task.CompletedTask;
    }
}
