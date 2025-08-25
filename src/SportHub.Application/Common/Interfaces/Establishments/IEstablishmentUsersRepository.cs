using Domain.Entities;
using Domain.Enums;

namespace Application.Common.Interfaces.Establishments;

public interface IEstablishmentUsersRepository
{
    Task AddAsync(EstablishmentUser establishmentUser, CancellationToken cancellationToken);
    Task<List<Guid>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken);
    Task<EstablishmentUser?> GetAsync(Guid userId, Guid establishmentId, CancellationToken cancellationToken);
    Task<bool> HasRoleAnywhereAsync(Guid userId, EstablishmentRole requiredRole, CancellationToken cancellationToken);
    Task AddManyAsync(IEnumerable<EstablishmentUser> establishmentUsers, CancellationToken cancellationToken);
    Task UpdateAsync(EstablishmentUser establishmentUser, CancellationToken cancellationToken);
}
