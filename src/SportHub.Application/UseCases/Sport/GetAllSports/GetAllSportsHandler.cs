using Application.Common.Interfaces;
using Application.Common.Models;
using Application.CQRS;

namespace Application.UseCases.Sport.GetAllSports;

public class GetAllSportsHandler : IQueryHandler<GetAllSportsQuery, PagedResult<SportSummaryResponse>>
{
    private readonly ISportsRepository _sportsRepository;

    public GetAllSportsHandler(ISportsRepository sportsRepository)
    {
        _sportsRepository = sportsRepository;
    }

    public async Task<Result<PagedResult<SportSummaryResponse>>> Handle(GetAllSportsQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;

        var pagedSports = await _sportsRepository.GetPagedAsync(
            page: filter.Page,
            pageSize: filter.PageSize,
            name: filter.Name,
            searchTerm: filter.SearchTerm);

        var result = new PagedResult<SportSummaryResponse>
        {
            Items = pagedSports.Items
                .Select(s => new SportSummaryResponse(s.Id, s.Name, s.Description, s.ImageUrl))
                .ToList(),
            TotalCount = pagedSports.TotalCount,
            Page = pagedSports.Page,
            PageSize = pagedSports.PageSize
        };

        return Result.Ok(result);
    }
}
