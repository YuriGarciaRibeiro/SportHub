using Domain.Common;
using Domain.Enums;
using SportHub.Domain.Common;

namespace Domain.Entities;

public class CourtMaintenance : TenantEntity, IEntity
{
    public Guid Id { get; set; }
    public Guid CourtId { get; set; }
    public Court? Court { get; set; }

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
