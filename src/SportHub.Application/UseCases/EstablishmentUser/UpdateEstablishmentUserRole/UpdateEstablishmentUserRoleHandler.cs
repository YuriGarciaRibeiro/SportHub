using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.CQRS;

namespace Application.UseCases.Establishments.UpdateEstablishmentUserRole;

public class UpdateEstablishmentUserRoleHandler : ICommandHandler<UpdateEstablishmentUserRoleCommand>
{
    public readonly IEstablishmentUsersRepository _establishmentUsersRepository;

    public UpdateEstablishmentUserRoleHandler(IEstablishmentUsersRepository establishmentUsersRepository)
    {
        _establishmentUsersRepository = establishmentUsersRepository;
    }

    public async Task<Result> Handle(UpdateEstablishmentUserRoleCommand request, CancellationToken cancellationToken)
    {
        var establishmentUser = await _establishmentUsersRepository.GetAsync(request.EstablishmentId, request.UserId);
        if (establishmentUser == null)
        {
            return Result.Fail(new NotFound("Establishment user not found."));
        }

        establishmentUser.Role = request.Request.Role;
        await _establishmentUsersRepository.UpdateAsync(establishmentUser);

        return Result.Ok();
    }
}
