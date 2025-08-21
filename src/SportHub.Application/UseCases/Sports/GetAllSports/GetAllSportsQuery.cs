using Application.CQRS;

namespace Application.UseCases.Sports.GetAllSports;

public class GetAllSportsQuery : IQuery<Result<GetAllSportsResponse>>
{
}
