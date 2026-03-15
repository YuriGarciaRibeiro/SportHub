using Application.Common.Interfaces;
using Application.Common.Models;
using Application.CQRS;
using Application.UseCases.Court.GetCourtById;
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
            searchTerm: filter.SearchTerm);

        var result = new PagedResult<CourtPublicResponse>
        {
            Items = pagedCourts.Items.Select(c => new CourtPublicResponse(
                c.Id,
                c.Name,
                c.ImageUrl,
                c.PricePerHour,
                c.SlotDurationMinutes,
                c.OpeningTime.ToString("HH:mm"),
                c.ClosingTime.ToString("HH:mm"),
                c.Sports.Select(s => new SportSummary(s.Id, s.Name)).ToList()
            )).ToList(),
            TotalCount = pagedCourts.TotalCount,
            Page = pagedCourts.Page,
            PageSize = pagedCourts.PageSize
        };

        return Result.Ok(result);
    }
}
