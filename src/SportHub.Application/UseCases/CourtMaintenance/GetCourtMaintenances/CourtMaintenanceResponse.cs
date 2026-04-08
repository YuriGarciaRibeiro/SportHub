using Domain.Enums;

namespace Application.UseCases.CourtMaintenance.GetCourtMaintenances;

public record CourtMaintenanceResponse(
    Guid Id,
    MaintenanceType Type,
    string? Description,
    DayOfWeek? DayOfWeek,
    DateOnly? Date,
    TimeOnly StartTime,
    TimeOnly EndTime);
