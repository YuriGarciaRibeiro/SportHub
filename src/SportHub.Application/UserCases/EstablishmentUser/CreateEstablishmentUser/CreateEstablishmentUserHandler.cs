using Application.Common.Interfaces;
using Application.CQRS;

namespace Application.UserCases.EstablishmentUser.CreateEstablishmentUser;

public class CreateEstablishmentUserHandler : ICommandHandler<CreateEstablishmentUserCommand, CreateEstablishmentUserResponse>
{
    private readonly IEstablishmentUsersRepository _repository;

    public CreateEstablishmentUserHandler(IEstablishmentUsersRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<CreateEstablishmentUserResponse>> Handle(CreateEstablishmentUserCommand request, CancellationToken cancellationToken)
    {
        var establishmentUser = new Domain.Entities.EstablishmentUser
        {
            UserId = request.UserId,
            EstablishmentId = request.EstablishmentId,
            Role = request.Role
        };

        await _repository.AddAsync(establishmentUser);

        return new CreateEstablishmentUserResponse(establishmentUser.UserId, establishmentUser.EstablishmentId, establishmentUser.Role);
    }
}
