using Application.CQRS;
using Domain.Enums;

namespace Application.UseCases.EstablishmentUser.CreateEstablishmentUser;

public record CreateEstablishmentUserCommand(
    Guid UserId,
    Guid EstablishmentId,
    EstablishmentRole Role) : ICommand<CreateEstablishmentUserResponse>;