using Application.Common.QueryFilters;

namespace Application.UseCases.Court.GetCourts;

public class GetCourtsQuery : ICommand<GetCourtsResponse>
{
    public CourtQueryFilter Filter { get; set; } = new CourtQueryFilter();
}
