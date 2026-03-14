using Application.CQRS;
using Application.UseCases.Court.CreateCourt;
using Application.UseCases.Court.GetCourtById;

namespace Application.UseCases.Court.UpdateCourt;

public class UpdateCourtCommand : ICommand<CourtPublicResponse>
{
    public Guid Id { get; set; }
    public CourtRequest Court { get; set; } = new CourtRequest();
}
