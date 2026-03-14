using Application.CQRS;

namespace Application.UseCases.Court.DeleteCourt;

public record DeleteCourtCommand(Guid Id) : ICommand;
