using Application.Common.Errors;
using Application.Common.Interfaces;

namespace SportHub.Application.UseCases.Auth.DeleteUser;

public class DeleteUserHandler : IRequestHandler<DeleteUserCommand, Result>
{
    private readonly IUsersRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;

    public DeleteUserHandler(IUsersRepository userRepository, ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
        _userRepository = userRepository;
    }

    public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
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

        await _userRepository.RemoveAsync(user);

        return Result.Ok();
    }
}