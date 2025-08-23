using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.CQRS;

namespace Application.UseCases.Establishments.UpdateEstablishmentUserRole;

public class UpdateEstablishmentUserRoleHandler : ICommandHandler<UpdateEstablishmentUserRoleCommand>
{
    private readonly IEstablishmentUserService _establishmentUserService;

    public UpdateEstablishmentUserRoleHandler(IEstablishmentUserService establishmentUserService)
    {
        _establishmentUserService = establishmentUserService;
    }

    public async Task<Result> Handle(UpdateEstablishmentUserRoleCommand request, CancellationToken cancellationToken)
    {
        var establishmentUser = await _establishmentUserService.GetAsync(request.UserId, request.EstablishmentId, cancellationToken);
        if (establishmentUser == null)
        {
            return Result.Fail(new NotFound("Establishment user not found."));
        }

        establishmentUser.Role = request.Request.Role;
        await _establishmentUserService.UpdateAsync(establishmentUser, ct: cancellationToken);

        return Result.Ok();
    }
}
