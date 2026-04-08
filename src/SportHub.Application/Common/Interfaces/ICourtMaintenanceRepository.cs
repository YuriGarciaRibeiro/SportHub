using Domain.Entities;

namespace Application.Common.Interfaces;

public interface ICourtMaintenanceRepository
{
    Task<CourtMaintenance?> GetByIdAsync(Guid id);
    Task<List<CourtMaintenance>> GetByCourtIdAsync(Guid courtId);
    Task AddAsync(CourtMaintenance entity);
    Task RemoveAsync(CourtMaintenance entity);
}
