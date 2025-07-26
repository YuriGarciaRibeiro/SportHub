using Domain.Enums;

namespace Application.UseCases.EstablishmentUser.CreateEstablishmentUser;

public record CreateEstablishmentUserResponse(
    IEnumerable<EstablishmentUserResponse> Users,
    Guid EstablishmentId
);

public record EstablishmentUserResponse(
    Guid UserId,
    EstablishmentRole Role
);