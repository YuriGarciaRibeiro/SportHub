using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.CQRS;
using Application.UseCases.Auth.GetCurrentUser;

namespace Application.UseCases.Auth.UpdateCurrentUser;

public class UpdateCurrentUserHandler : ICommandHandler<UpdateCurrentUserCommand, UserProfileResponse>
{
    private readonly IUsersRepository _usersRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCurrentUserHandler(
        IUsersRepository usersRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _usersRepository = usersRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UserProfileResponse>> Handle(UpdateCurrentUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _usersRepository.GetByIdAsync(_currentUserService.UserId);

        if (user is null)
            return Result.Fail(new NotFound("Usuário não encontrado."));

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();

        await _usersRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok(new UserProfileResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            $"{user.FirstName} {user.LastName}".Trim(),
            user.Email,
            user.Role.ToString(),
            user.LastLoginAt,
            user.CreatedAt,
            0m
        ));
    }
}
