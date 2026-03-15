using Application.Common.Models;
using Application.CQRS;

namespace Application.UseCases.Sport.GetAllSports;

public record GetAllSportsQuery(GetSportsFilter Filter) : IQuery<PagedResult<SportSummaryResponse>>;

public class GetSportsFilter
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Name { get; set; }
    public string? SearchTerm { get; set; }
}
