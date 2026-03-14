using Application.CQRS;
using Application.UseCases.Auth.GetCurrentUser;

namespace Application.UseCases.Auth.UpdateCurrentUser;

public record UpdateCurrentUserCommand(string FirstName, string LastName) : ICommand<UserProfileResponse>;
