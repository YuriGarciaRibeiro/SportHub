using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IEstablishmentService
{
    Task<Result<Establishment>> GetEstablishmentByIdAsync(Guid id);
    Task<Result<List<Establishment>>> GetEstablishmentsByOwnerIdAsync(Guid ownerId);
    Task<Result> CreateEstablishmentAsync(Establishment request);
    Task<Result> UpdateEstablishmentAsync(Establishment request);
    Task<Result> DeleteEstablishmentAsync(Guid id);
}
