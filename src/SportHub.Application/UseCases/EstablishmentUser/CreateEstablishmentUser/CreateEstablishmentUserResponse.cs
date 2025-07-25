using Domain.Enums;

namespace Application.UseCases.EstablishmentUser.CreateEstablishmentUser;

public record CreateEstablishmentUserResponse(
    Guid UserId,
    Guid EstablishmentId,
    EstablishmentRole Role
);
