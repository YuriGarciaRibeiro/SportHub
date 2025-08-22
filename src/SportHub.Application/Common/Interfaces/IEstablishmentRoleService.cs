using Domain.Enums;

namespace Application.Common.Interfaces;

public interface IEstablishmentRoleService
{
    Task<EstablishmentRole?> GetRoleAsync(Guid userId, Guid establishmentId, CancellationToken cancellationToken);
    Task<bool> HasAtLeastRoleAsync(Guid userId, Guid establishmentId, EstablishmentRole required, CancellationToken cancellationToken);
    Task<bool> HasRoleAnywhereAsync(Guid userId, EstablishmentRole requiredRole, CancellationToken cancellationToken);
    Task<Result> ValidateUserPermissionAsync(Guid userId, Guid establishmentId, EstablishmentRole minimumRole, CancellationToken cancellationToken);
}