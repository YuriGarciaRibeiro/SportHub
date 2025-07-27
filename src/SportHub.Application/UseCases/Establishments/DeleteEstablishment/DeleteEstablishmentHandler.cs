using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.CQRS;

namespace Application.UseCases.Establishments.DeleteEstablishment;

public class DeleteEstablishmentHandler : ICommandHandler<DeleteEstablishmentCommand>
{
    private readonly IEstablishmentsRepository _repository;

    public DeleteEstablishmentHandler(IEstablishmentsRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(DeleteEstablishmentCommand request, CancellationToken cancellationToken)
    {
        var establishment = await _repository.GetByIdWithAddressAsync(request.EstablishmentId);
        if (establishment == null)
        {
            return Result.Fail(new NotFound("Establishment not found."));
        }
        if (establishment.IsDeleted)
        {
            return Result.Fail(new Conflict("Establishment is already deleted."));
        }

        await _repository.RemoveAsync(establishment);
        
        return Result.Ok();
    }
}