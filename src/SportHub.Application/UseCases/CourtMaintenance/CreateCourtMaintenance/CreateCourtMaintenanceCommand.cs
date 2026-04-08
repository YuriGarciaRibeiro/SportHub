using Application.CQRS;
using Domain.Enums;

namespace Application.UseCases.CourtMaintenance.CreateCourtMaintenance;

public class CreateCourtMaintenanceCommand : ICommand<Guid>
{
    public Guid CourtId { get; set; }
    public MaintenanceType Type { get; set; }
    public string? Description { get; set; }

    // Recorrente
    public DayOfWeek? DayOfWeek { get; set; }

    // Pontual
    public DateOnly? Date { get; set; }

    // Ambos
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
}
