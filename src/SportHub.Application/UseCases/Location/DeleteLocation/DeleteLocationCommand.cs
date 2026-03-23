using Application.CQRS;
using MediatR;

namespace Application.UseCases.Location.DeleteLocation;

public record DeleteLocationCommand(Guid Id) : ICommand<Unit>;
