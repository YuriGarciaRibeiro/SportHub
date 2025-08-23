using Domain.Entities;
using Domain.Enums;

namespace Application.Common.Interfaces;

public interface IEstablishmentUserService
{
    Task<List<Guid>> GetByOwnerIdAsync(Guid ownerId, CancellationToken ct = default);
    Task<EstablishmentUser?> GetAsync(Guid userId, Guid establishmentId, CancellationToken ct = default);
    Task<bool> HasRoleAnywhereAsync(Guid userId, EstablishmentRole requiredRole, CancellationToken ct = default);
    Task AddManyAsync(IEnumerable<EstablishmentUser> establishmentUsers, CancellationToken ct = default);
    Task<EstablishmentUser> CreateAsync(EstablishmentUser entity, CancellationToken ct = default);
    Task UpdateAsync(EstablishmentUser entity, CancellationToken ct = default);
}
