using Application.Common.Models;
using Application.CQRS;
using Application.UseCases.Court.GetCourtById;

namespace Application.UseCases.Court.GetAllCourts;

public record GetAllCourtsQuery(GetCourtsFilter Filter) : IQuery<PagedResult<CourtPublicResponse>>;

public class GetCourtsFilter
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Name { get; set; }
    public Guid? SportId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? SearchTerm { get; set; }
    public Guid? LocationId { get; set; }
}
