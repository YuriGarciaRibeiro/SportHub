using Application.CQRS;

namespace Application.UseCases.Sport.UpdateSport;

public class UpdateSportCommand : ICommand<UpdateSportResponse>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
}
