using Application.CQRS;

namespace Application.UseCases.Sport.GetAllSports;

public class GetAllSportsQuery : IQuery<List<SportSummaryResponse>>
{
}
