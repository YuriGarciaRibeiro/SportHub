using Application.Common.Errors;
using Application.Common.Interfaces;
using Domain.Enums;
using FluentResults;

namespace Application.Services;
public class EstablishmentRoleService : IEstablishmentRoleService
{
    private readonly IEstablishmentUsersRepository _repo;
    public EstablishmentRoleService(IEstablishmentUsersRepository repo) 
        => _repo = repo;

    public async Task<EstablishmentRole?> GetRoleAsync(Guid userId, Guid estId)
    {
        var ue = await _repo.GetAsync(userId, estId);
        return ue?.Role;
    }

    public async Task<bool> HasAtLeastRoleAsync(
        Guid userId, Guid estId, EstablishmentRole required)
    {
        var actual = await GetRoleAsync(userId, estId);
        return actual.HasValue && actual.Value >= required;
    }

    public async Task<Result> ValidateUserPermissionAsync(Guid userId, Guid establishmentId, EstablishmentRole minimumRole)
    {
        var hasPermission = await HasAtLeastRoleAsync(userId, establishmentId, minimumRole);
        
        return hasPermission 
            ? Result.Ok() 
            : Result.Fail(new Forbidden("Você não tem permissão para realizar esta ação neste estabelecimento."));
    }
}