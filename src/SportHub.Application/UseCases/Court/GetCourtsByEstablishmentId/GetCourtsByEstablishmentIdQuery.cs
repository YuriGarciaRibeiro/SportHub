using Application.CQRS;

namespace Application.UseCases.Court.GetCourtsByEstablishmentId;

public class GetCourtsByEstablishmentIdQuery : IQuery<GetCourtsByEstablishmentIdResponse>
{
    public Guid EstablishmentId { get; set; }

    public GetCourtsByEstablishmentIdQuery(Guid establishmentId)
    {
        EstablishmentId = establishmentId;
    }
}
