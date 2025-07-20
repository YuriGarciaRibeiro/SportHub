using Application.CQRS;

namespace Application.UserCases.Establishments.GetEstablishmentById;

public record GetEstablishmentByIdQuery(Guid Id) : IQuery<GetEstablishmentByIdResponse>
{
    public Guid Id { get; init; } = Id;
};
