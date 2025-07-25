using Application.CQRS;

namespace Application.UseCases.Establishments.GetEstablishmentByOwnerId;

public record GetEstablishmentByOwnerIdQuery() : IQuery<GetEstablishmentsByOwnerIdResponse>
{
};
