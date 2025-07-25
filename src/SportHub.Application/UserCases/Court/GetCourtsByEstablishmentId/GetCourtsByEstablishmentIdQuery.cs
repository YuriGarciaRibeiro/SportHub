using Application.CQRS;

namespace Application.UserCases.Court.GetCourtsByEstablishmentId;

public class GetCourtsByEstablishmentIdQuery : IQuery<GetCourtsByEstablishmentIdResponse>
{
    public Guid EstablishmentId { get; set; }

    public GetCourtsByEstablishmentIdQuery(Guid establishmentId)
    {
        EstablishmentId = establishmentId;
    }
}
