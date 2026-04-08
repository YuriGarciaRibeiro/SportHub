using Application.Common.Interfaces;
using Application.CQRS;

namespace Application.UseCases.CourtMaintenance.GetCourtMaintenances;

public class GetCourtMaintenancesHandler : IQueryHandler<GetCourtMaintenancesQuery, List<CourtMaintenanceResponse>>
{
    private readonly ICourtMaintenanceRepository _maintenanceRepository;

    public GetCourtMaintenancesHandler(ICourtMaintenanceRepository maintenanceRepository)
    {
        _maintenanceRepository = maintenanceRepository;
    }

    public async Task<Result<List<CourtMaintenanceResponse>>> Handle(GetCourtMaintenancesQuery request, CancellationToken cancellationToken)
    {
        var maintenances = await _maintenanceRepository.GetByCourtIdAsync(request.CourtId);

        var response = maintenances
            .Select(m => new CourtMaintenanceResponse(
                m.Id,
                m.Type,
                m.Description,
                m.DayOfWeek,
                m.Date,
                m.StartTime,
                m.EndTime))
            .ToList();

        return Result.Ok(response);
    }
}
