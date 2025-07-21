using Domain.Enums;

namespace Application.Common.Interfaces;

public interface IEstablishmentRoleService
{
    Task<EstablishmentRole?> GetRoleAsync(Guid userId, Guid establishmentId);
    Task<bool> HasAtLeastRoleAsync(Guid userId, Guid establishmentId, EstablishmentRole required);
}