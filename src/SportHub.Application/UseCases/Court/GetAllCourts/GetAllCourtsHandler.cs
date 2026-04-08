using Application.Common.Interfaces;
using Application.Common.Models;
using Application.CQRS;
using Application.UseCases.Court.GetCourtById;
using Application.UseCases.CourtMaintenance.GetCourtMaintenances;
using FluentResults;

namespace Application.UseCases.Court.GetAllCourts;

public class GetAllCourtsHandler : IQueryHandler<GetAllCourtsQuery, PagedResult<CourtPublicResponse>>
{
    private readonly ICourtsRepository _courtsRepo;

    public GetAllCourtsHandler(ICourtsRepository courtsRepo)
    {
        _courtsRepo = courtsRepo;
    }

    public async Task<Result<PagedResult<CourtPublicResponse>>> Handle(
        GetAllCourtsQuery request, CancellationToken ct)
    {
        var filter = request.Filter;

        var pagedCourts = await _courtsRepo.GetPagedAsync(
            page: filter.Page,
            pageSize: filter.PageSize,
            name: filter.Name,
            sportId: filter.SportId,
            minPrice: filter.MinPrice,
            maxPrice: filter.MaxPrice,
            searchTerm: filter.SearchTerm,
            locationId: filter.LocationId);

        var result = new PagedResult<CourtPublicResponse>
        {
            Items = pagedCourts.Items.Select(c => new CourtPublicResponse(
                c.Id,
                c.Name,
                c.ImageUrl,
                c.ImageUrls,
                c.PricePerHour,
                c.SlotDurationMinutes,
                c.MinBookingSlots,
                c.MaxBookingSlots,
                c.OpeningTime.ToString("HH:mm"),
                c.ClosingTime.ToString("HH:mm"),
                c.Amenities,
                c.Sports.Select(s => new SportSummary(s.Id, s.Name)).ToList(),
                c.LocationId,
                c.Location?.Name,
                c.PeakPricePerHour,
                c.PeakStartTime,
                c.PeakEndTime,
                c.Maintenances.Select(m => new CourtMaintenanceResponse(
                    m.Id, m.Type, m.Description, m.DayOfWeek, m.Date, m.StartTime, m.EndTime)).ToList()
            )).ToList(),
            TotalCount = pagedCourts.TotalCount,
            Page = pagedCourts.Page,
            PageSize = pagedCourts.PageSize
        };

        return Result.Ok(result);
    }
}
