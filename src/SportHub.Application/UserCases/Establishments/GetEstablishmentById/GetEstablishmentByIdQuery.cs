using Application.CQRS;

public record GetEstablishmentByIdQuery(Guid Id) : IQuery<GetEstablishmentByIdResponse>
{
    public Guid Id { get; init; } = Id;
};
