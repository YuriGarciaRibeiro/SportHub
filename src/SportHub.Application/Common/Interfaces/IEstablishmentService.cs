using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IEstablishmentService : IBaseService<Establishment>
{
    Task<Result<List<Establishment>>> GetEstablishmentsByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken);
}
