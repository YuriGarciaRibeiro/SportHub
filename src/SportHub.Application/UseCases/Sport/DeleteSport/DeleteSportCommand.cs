using Application.CQRS;

namespace Application.UseCases.Sport.DeleteSport;

public class DeleteSportCommand : ICommand
{
    public Guid Id { get; set; }
}
