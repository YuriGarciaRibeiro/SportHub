namespace Application.UseCases.Court.GetCourt;

public class GetCourtQuery : IQuery<GetCourtResponse>
{
    public Guid CourtId { get; set; }
}
