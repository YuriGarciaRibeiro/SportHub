using Application.Common.Errors;
using Application.Common.Interfaces;
using Domain.Enums;

namespace Application.Services;

public class EstablishmentRoleService : IEstablishmentRoleService
{
    private readonly IEstablishmentUsersRepository _repo;
    public EstablishmentRoleService(IEstablishmentUsersRepository repo)
        => _repo = repo;

    public async Task<EstablishmentRole?> GetRoleAsync(Guid userId, Guid estId, CancellationToken cancellationToken)
    {
        var ue = await _repo.GetAsync(userId, estId, cancellationToken);
        return ue?.Role;
    }

    public async Task<bool> HasAtLeastRoleAsync(
        Guid userId, Guid estId, EstablishmentRole required, CancellationToken cancellationToken)
    {
        var actual = await GetRoleAsync(userId, estId, cancellationToken);
        return actual.HasValue && actual.Value >= required;
    }

    public Task<bool> HasRoleAnywhereAsync(Guid userId, EstablishmentRole requiredRole, CancellationToken cancellationToken)
    {
        return _repo.HasRoleAnywhereAsync(userId, requiredRole, cancellationToken);
    }

    public async Task<Result> ValidateUserPermissionAsync(Guid userId, Guid establishmentId, EstablishmentRole minimumRole, CancellationToken cancellationToken)
    {
        var hasPermission = await HasAtLeastRoleAsync(userId, establishmentId, minimumRole, cancellationToken);

        return hasPermission
            ? Result.Ok()
            : Result.Fail(new Forbidden("Você não tem permissão para realizar esta ação neste estabelecimento."));
    }
    
    
}