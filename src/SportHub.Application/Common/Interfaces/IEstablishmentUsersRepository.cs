using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IEstablishmentUsersRepository
{
    Task AddAsync(EstablishmentUser establishmentUser);
    Task<EstablishmentUser?> GetByIdAsync(Guid id);
    Task<List<EstablishmentUser>> GetByEstablishmentIdAsync(Guid establishmentId);
    Task<List<EstablishmentUser>> GetByUserIdAsync(Guid userId);
    Task<List<string>> GetByOwnerIdAsync(Guid ownerId);
}
