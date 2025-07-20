using Application.CQRS;

namespace Application.UserCases.Establishments.GetEstablishmentByOwnerId;

public record GetEstablishmentByOwnerIdQuery() : IQuery<GetEstablishmentsByOwnerIdResponse>
{
};
