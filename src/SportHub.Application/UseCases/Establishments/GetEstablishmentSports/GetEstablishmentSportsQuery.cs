namespace Application.UseCases.Establishments.GetEstablishmentSports;

public class GetEstablishmentSportsQuery : IQuery<GetEstablishmentSportsResponse>
{
    public Guid EstablishmentId { get; set; }
}

