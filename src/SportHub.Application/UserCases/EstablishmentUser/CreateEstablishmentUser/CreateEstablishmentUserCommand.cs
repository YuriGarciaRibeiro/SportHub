using Application.CQRS;
using Domain.Enums;

namespace Application.UserCases.EstablishmentUser.CreateEstablishmentUser;

public record CreateEstablishmentUserCommand(
    Guid UserId,
    Guid EstablishmentId,
    EstablishmentRole Role) : ICommand<CreateEstablishmentUserResponse>;