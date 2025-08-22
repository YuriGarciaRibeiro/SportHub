namespace Application.Common.QueryFilters;

public class CourtQueryFilter
{
    public string? Name { get; set; }
    public TimeOnly? OpeningTime { get; set; }
    public TimeOnly? ClosingTime { get; set; }
    public IEnumerable<Guid>? SportIds { get; set; }
}
