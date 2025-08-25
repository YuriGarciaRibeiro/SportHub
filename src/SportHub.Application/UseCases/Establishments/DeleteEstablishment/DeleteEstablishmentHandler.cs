using Application.Common.Errors;
using Application.CQRS;

namespace Application.UseCases.Establishments.DeleteEstablishment;

public class DeleteEstablishmentHandler : ICommandHandler<DeleteEstablishmentCommand>
{
    private readonly IEstablishmentService _establishmentService;

    public DeleteEstablishmentHandler(IEstablishmentService establishmentService)
    {
        _establishmentService = establishmentService;
    }

    public async Task<Result> Handle(DeleteEstablishmentCommand request, CancellationToken cancellationToken)
    {
        var establishment = await _establishmentService.GetByIdAsync(request.EstablishmentId, cancellationToken);
        if (establishment == null)
        {
            return Result.Fail(new NotFound("Establishment not found."));
        }
        if (establishment.IsDeleted)
        {
            return Result.Fail(new Conflict("Establishment is already deleted."));
        }

        await _establishmentService.DeleteAsync(establishment, cancellationToken);

        return Result.Ok();
    }
}