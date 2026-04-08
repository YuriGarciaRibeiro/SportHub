using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.CQRS;
using Application.UseCases.CourtMaintenance.GetCourtMaintenances;

namespace Application.UseCases.Court.GetCourtById;

public class GetCourtByIdHandler : IQueryHandler<GetCourtByIdQuery, CourtPublicResponse>
{
    private readonly ICourtsRepository _courtRepository;

    public GetCourtByIdHandler(ICourtsRepository courtRepository)
    {
        _courtRepository = courtRepository;
    }

    public async Task<Result<CourtPublicResponse>> Handle(GetCourtByIdQuery request, CancellationToken cancellationToken)
    {
        var court = await _courtRepository.GetByIdAsync(request.Id, new GetCourtIncludeSettings
        {
            IncludeLocation = true,
            IncludeSports = true,
            IncludeMaintenances = true,
            AsNoTracking = true
        });

        if (court is null)
        {
            return Result.Fail(new NotFound($"Quadra com ID {request.Id} não encontrada."));
        }

        var response = new CourtPublicResponse(
            court.Id,
            court.Name,
            court.ImageUrl,
            court.ImageUrls,
            court.PricePerHour,
            court.SlotDurationMinutes,
            court.MinBookingSlots,
            court.MaxBookingSlots,
            court.OpeningTime.ToString("HH:mm"),
            court.ClosingTime.ToString("HH:mm"),
            court.Amenities,
            court.Sports.Select(s => new SportSummary(s.Id, s.Name)).ToList(),
            court.LocationId,
            court.Location?.Name,
            court.PeakPricePerHour,
            court.PeakStartTime,
            court.PeakEndTime,
            court.Maintenances.Select(m => new CourtMaintenanceResponse(
                m.Id, m.Type, m.Description, m.DayOfWeek, m.Date, m.StartTime, m.EndTime)).ToList()
        );

        return Result.Ok(response);
    }
}
