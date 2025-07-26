using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IEstablishmentsRepository
{
    Task<Establishment?> GetByIdAsync(Guid id);
    Task<List<Establishment>> GetAllAsync();
    Task AddAsync(Establishment establishment);
    Task UpdateAsync(Establishment establishment);
    Task DeleteAsync(Guid id);
    Task<List<Establishment>> GetByIdsAsync(IEnumerable<Guid> ids);
}
