using Application.CQRS;

namespace Application.UseCases.CourtMaintenance.GetCourtMaintenances;

public class GetCourtMaintenancesQuery : IQuery<List<CourtMaintenanceResponse>>
{
    public Guid CourtId { get; set; }
}
