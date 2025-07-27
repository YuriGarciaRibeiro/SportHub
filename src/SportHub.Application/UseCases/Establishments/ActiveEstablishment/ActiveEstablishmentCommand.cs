using Application.CQRS;

namespace Application.UseCases.Establishments.ActiveEstablishment;

public record ActiveEstablishmentCommand(
    Guid UserId
) : ICommand;