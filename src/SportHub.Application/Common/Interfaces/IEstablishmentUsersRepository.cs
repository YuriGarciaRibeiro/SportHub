using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IEstablishmentUsersRepository
{
    Task AddAsync(EstablishmentUser establishmentUser);
    Task<List<string>> GetByOwnerIdAsync(Guid ownerId);
    Task<EstablishmentUser?> GetAsync(Guid userId, Guid establishmentId);
}
