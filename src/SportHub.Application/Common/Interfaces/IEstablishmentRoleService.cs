using Domain.Enums;
using FluentResults;

namespace Application.Common.Interfaces;

public interface IEstablishmentRoleService
{
    Task<EstablishmentRole?> GetRoleAsync(Guid userId, Guid establishmentId);
    Task<bool> HasAtLeastRoleAsync(Guid userId, Guid establishmentId, EstablishmentRole required);
    Task<bool> HasRoleAnywhereAsync(Guid userId, EstablishmentRole requiredRole);
    Task<Result> ValidateUserPermissionAsync(Guid userId, Guid establishmentId, EstablishmentRole minimumRole);
}