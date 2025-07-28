namespace SportHub.Application.UseCases.Auth.DeleteUser;

public record DeleteUserCommand(Guid UserId) : IRequest<Result>;