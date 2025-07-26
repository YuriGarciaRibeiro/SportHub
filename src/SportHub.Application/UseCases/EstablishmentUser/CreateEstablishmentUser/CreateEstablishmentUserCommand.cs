using Application.CQRS;
using Domain.Enums;

namespace Application.UseCases.EstablishmentUser.CreateEstablishmentUser;

public record CreateEstablishmentUserCommand(
    IEnumerable<EstablishmentUserRequest> Users,
    Guid EstablishmentId) : ICommand<CreateEstablishmentUserResponse>;

public record EstablishmentUserRequest(
    Guid UserId,
    EstablishmentRole Role
);