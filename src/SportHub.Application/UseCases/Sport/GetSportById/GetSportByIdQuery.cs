using Application.CQRS;
using Application.UseCases.Sport.GetAllSports;

namespace Application.UseCases.Sport.GetSportById;

public record GetSportByIdQuery(Guid Id) : IQuery<SportSummaryResponse>;
