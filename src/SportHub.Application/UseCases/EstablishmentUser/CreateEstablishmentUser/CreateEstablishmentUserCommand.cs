using Application.CQRS;
using Domain.Enums;

namespace Application.UseCases.EstablishmentUser.CreateEstablishmentUser;

public record CreateEstablishmentUserCommand(
    CreateEstablishmentUserRequest Request,
    Guid EstablishmentId) : ICommand<CreateEstablishmentUserResponse>;

public record CreateEstablishmentUserRequest(
    IEnumerable<EstablishmentUserRequest> Users
);

public record EstablishmentUserRequest(
    Guid UserId,
    EstablishmentRole Role
);