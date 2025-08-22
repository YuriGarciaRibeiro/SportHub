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

    public Task<Result> CreateEstablishmentAsync(Establishment request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<Result> DeleteEstablishmentAsync(Guid id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<Establishment>> GetEstablishmentByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var establishment = await _establishmentRepository.GetByIdAsync(id, cancellationToken);
        return establishment != null
            ? Result.Ok(establishment)
            : Result.Fail("Establishment not found.");
    }

    public async Task<Result<List<Establishment>>> GetEstablishmentsByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken)
    {
        var establishmentsId = await _establishmentUsersRepository.GetByOwnerIdAsync(ownerId, cancellationToken);
        if (establishmentsId == null)
        {
            return Result.Fail("No establishments found for the given owner ID.");
        }

        var establishments = await _establishmentRepository.GetByIdsWithDetailsAsync(establishmentsId, cancellationToken);

        return Result.Ok(establishments);
    }

    public Task<Result> UpdateEstablishmentAsync(Establishment request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
