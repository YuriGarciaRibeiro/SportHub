using Application.CQRS;

namespace Application.UseCases.CourtMaintenance.DeleteCourtMaintenance;

public class DeleteCourtMaintenanceCommand : ICommand
{
    public Guid CourtId { get; set; }
    public Guid MaintenanceId { get; set; }
}
