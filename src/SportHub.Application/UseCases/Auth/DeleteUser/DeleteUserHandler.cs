using Application.Common.Errors;
using Domain.Entities;

namespace SportHub.Application.UseCases.Auth.DeleteUser;

public class DeleteUserHandler : IRequestHandler<DeleteUserCommand, Result>
{
    private readonly IUserService _userService;
    private readonly ICurrentUserService _currentUserService;

    public DeleteUserHandler(IUserService userService, ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
        _userService = userService;
    }

    public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userService.GetByIdAsync(request.UserId, ct: cancellationToken);
        var currentUser = _currentUserService.UserId;
        
        if (user == null)
        {
            return Result.Fail(new NotFound("User not found."));
        }

        if (user.Id != currentUser)
        {
            return Result.Fail(new NotFound("You can only delete your own account."));
        }

        if (user.IsDeleted)
        {
            return Result.Fail(new Conflict("User account is already deleted."));
        }

        await _userService.DeleteAsync(user, cancellationToken);

        return Result.Ok();
    }
}