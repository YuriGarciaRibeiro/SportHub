using Domain.Entities;
using Domain.Enums;

namespace Application.Common.Interfaces;

public interface IEstablishmentsRepository
{
    Task<Establishment?> GetByIdAsync(Guid id);
    Task<List<Establishment>> GetAllAsync();
    Task AddAsync(Establishment establishment);
    Task UpdateAsync(Establishment establishment);
    Task DeleteAsync(Guid id);
}
