using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.CQRS;

namespace Application.UseCases.Establishments.ActiveEstablishment;

public class ActiveEstablishmentHandler : ICommandHandler<ActiveEstablishmentCommand>
{
    private readonly IEstablishmentService _establishmentService;

    public ActiveEstablishmentHandler(IEstablishmentService establishmentService)
    {
        _establishmentService = establishmentService;
    }

    public async Task<Result> Handle(ActiveEstablishmentCommand request, CancellationToken cancellationToken)
    {
        var establishment = await _establishmentService.GetByIdAsync(request.UserId, cancellationToken);
        if (establishment == null)
        {
            return Result.Fail(new NotFound("Establishment not found."));
        }

        if (!establishment.IsDeleted)
        {
            return Result.Fail(new Conflict("Establishment is not deleted."));
        }

        establishment.Restore();
        await _establishmentService.UpdateAsync(establishment, cancellationToken);

        return Result.Ok();
    }
}
