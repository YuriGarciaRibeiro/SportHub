using Domain.Entities;
using Domain.Enums;

namespace Application.Common.Interfaces;

public interface IEstablishmentUsersRepository
{
    Task AddAsync(EstablishmentUser establishmentUser);
    Task<List<Guid>> GetByOwnerIdAsync(Guid ownerId);
    Task<EstablishmentUser?> GetAsync(Guid userId, Guid establishmentId);
    Task<bool> HasRoleAnywhereAsync(Guid userId, EstablishmentRole requiredRole);
    Task AddManyAsync(IEnumerable<EstablishmentUser> establishmentUsers);
    Task UpdateAsync(EstablishmentUser establishmentUser);
}
