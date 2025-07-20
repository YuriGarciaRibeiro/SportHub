using Domain.Enums;

namespace Application.UserCases.EstablishmentUser.CreateEstablishmentUser;

public record CreateEstablishmentUserResponse(
    Guid UserId,
    Guid EstablishmentId,
    EstablishmentRole Role
);
