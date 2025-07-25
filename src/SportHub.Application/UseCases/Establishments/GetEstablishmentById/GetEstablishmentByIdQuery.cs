using Application.CQRS;

namespace Application.UseCases.Establishments.GetEstablishmentById;

public record GetEstablishmentByIdQuery(Guid Id) : IQuery<GetEstablishmentByIdResponse>
{
    public Guid Id { get; init; } = Id;
};
