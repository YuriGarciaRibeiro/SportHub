using Application.Common.Interfaces;
using Domain.Entities;

namespace Application.Services;

public class EstablishmentService : IEstablishmentService
{
    private readonly IEstablishmentsRepository _establishmentRepository;
    private readonly IEstablishmentUsersRepository _establishmentUsersRepository;

    public EstablishmentService(IEstablishmentsRepository establishmentRepository, IEstablishmentUsersRepository establishmentUsersRepository)
    {
        _establishmentUsersRepository = establishmentUsersRepository;
        _establishmentRepository = establishmentRepository;
    }

    public Task<Result> CreateEstablishmentAsync(Establishment request)
    {
        throw new NotImplementedException();
    }

    public Task<Result> DeleteEstablishmentAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<Establishment>> GetEstablishmentByIdAsync(Guid id)
    {
        var establishment = await _establishmentRepository.GetByIdAsync(id);
        return establishment != null
            ? Result.Ok(establishment)
            : Result.Fail("Establishment not found.");
    }

    public async Task<Result<List<Establishment>>> GetEstablishmentsByOwnerIdAsync(Guid ownerId)
    {
        var establishmentsId = await _establishmentUsersRepository.GetByOwnerIdAsync(ownerId);
        if (establishmentsId == null)
        {
            return Result.Fail("No establishments found for the given owner ID.");
        }

        var establishments = await _establishmentRepository.GetByIdsAsync(establishmentsId);

        return Result.Ok(establishments);
    }

    public Task<Result> UpdateEstablishmentAsync(Establishment request)
    {
        throw new NotImplementedException();
    }
}
