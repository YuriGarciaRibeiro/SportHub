using Application.Common.Interfaces;
using Domain.Entities;
using FluentResults;

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

    public Task<Result<Establishment>> GetEstablishmentByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<List<Establishment>>> GetEstablishmentsByOwnerIdAsync(Guid ownerId)
    {
        var establishmentsId = await _establishmentUsersRepository.GetByOwnerIdAsync(ownerId);
        if (establishmentsId == null || !establishmentsId.Any())
        {
            return Result.Fail("No establishments found for the given owner ID.");
        }

        var establishments = new List<Establishment>();

        foreach (var id in establishmentsId)
        {
            var establishment = await _establishmentRepository.GetByIdAsync(Guid.Parse(id));
            if (establishment == null)
            {
                return Result.Fail($"Establishment with ID {id} not found.");
            }
            establishments.Add(establishment);
        }

        return Result.Ok(establishments);
    }

    public Task<Result> UpdateEstablishmentAsync(Establishment request)
    {
        throw new NotImplementedException();
    }
}
