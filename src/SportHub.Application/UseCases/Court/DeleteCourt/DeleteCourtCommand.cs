using Application.CQRS;

public class DeleteCourtCommand : ICommand
{
    public Guid Id { get; set; }
}
