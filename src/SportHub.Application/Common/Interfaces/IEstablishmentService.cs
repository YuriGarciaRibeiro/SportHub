using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IEstablishmentService
{
    Task<Result<List<Establishment>>> GetEstablishmentsByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken);
    Task<Result> CreateEstablishmentAsync(Establishment request, CancellationToken cancellationToken);
    Task<Result> UpdateEstablishmentAsync(Establishment request, CancellationToken cancellationToken);
    Task<Result> DeleteEstablishmentAsync(Guid id, CancellationToken cancellationToken);
    Task<Result<Establishment>> GetEstablishmentByIdAsync(Guid id, CancellationToken cancellationToken);
}
