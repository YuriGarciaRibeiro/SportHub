using Application.CQRS;

public record GetEstablishmentQuery(Guid Id) : IQuery<GetEstablishmentResponse>
{
    public Guid Id { get; init; } = Id;
};
