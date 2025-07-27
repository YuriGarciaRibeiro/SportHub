using Application.CQRS;

namespace Application.UseCases.Establishments.DeleteEstablishment;

public record DeleteEstablishmentCommand(Guid EstablishmentId) : ICommand;