using Application.CQRS;

namespace Application.UseCases.Sport.CreateSport;

public class CreateSportCommand : ICommand<CreateSportResponse>
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
}
