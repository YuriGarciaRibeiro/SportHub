using Application.CQRS;
using Application.UseCases.Court.GetCourtById;

namespace Application.UseCases.Court.GetAllCourts;

public record GetAllCourtsQuery : IQuery<List<CourtPublicResponse>>;
