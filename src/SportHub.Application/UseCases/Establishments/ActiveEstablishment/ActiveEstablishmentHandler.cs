using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.CQRS;

namespace Application.UseCases.Establishments.ActiveEstablishment;

public class ActiveEstablishmentHandler : ICommandHandler<ActiveEstablishmentCommand>
{
    private readonly IEstablishmentsRepository _establishmentsRepository;

    public ActiveEstablishmentHandler(IEstablishmentsRepository establishmentsRepository)
    {
        _establishmentsRepository = establishmentsRepository;
    }

    public async Task<Result> Handle(ActiveEstablishmentCommand request, CancellationToken cancellationToken)
    {
        var establishment = await _establishmentsRepository.GetByIdWithAddressAsync(request.UserId);
        if (establishment == null)
        {
            return Result.Fail(new NotFound("Establishment not found."));
        }

        if (!establishment.IsDeleted)
        {
            return Result.Fail(new Conflict("Establishment is not deleted."));
        }

        establishment.Restore();
        await _establishmentsRepository.UpdateAsync(establishment);

        return Result.Ok();
    }
}
