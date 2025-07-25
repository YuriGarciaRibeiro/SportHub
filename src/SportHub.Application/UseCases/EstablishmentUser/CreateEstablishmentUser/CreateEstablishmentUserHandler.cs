using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.CQRS;
using Domain.Enums;

namespace Application.UseCases.EstablishmentUser.CreateEstablishmentUser;

public class CreateEstablishmentUserHandler : ICommandHandler<CreateEstablishmentUserCommand, CreateEstablishmentUserResponse>
{
    private readonly IEstablishmentUsersRepository _repository;
    private readonly IUserService _userService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEstablishmentRoleService _establishmentRoleService;

    public CreateEstablishmentUserHandler(
        IEstablishmentUsersRepository repository, 
        IUserService userService,
        ICurrentUserService currentUserService,
        IEstablishmentRoleService establishmentRoleService)
    {
        _repository = repository;
        _userService = userService;
        _currentUserService = currentUserService;
        _establishmentRoleService = establishmentRoleService;
    }

    public async Task<Result<CreateEstablishmentUserResponse>> Handle(CreateEstablishmentUserCommand request, CancellationToken cancellationToken)
    {
        var permissionResult = await _establishmentRoleService.ValidateUserPermissionAsync(
            _currentUserService.UserId, 
            request.EstablishmentId, 
            EstablishmentRole.Staff);
            
        if (permissionResult.IsFailed)
        {
            return Result.Fail(permissionResult.Errors);
        }

        var currentUser = await _userService.GetUserByIdAsync(request.UserId);
        if (currentUser == null)
        {
            return Result.Fail(new NotFound($"User with ID '{request.UserId}' not found."));
        }

        var establishmentUser = new Domain.Entities.EstablishmentUser
        {
            UserId = request.UserId,
            EstablishmentId = request.EstablishmentId,
            Role = request.Role
        };

        await _repository.AddAsync(establishmentUser);

        if (currentUser.Role == UserRole.User)
        {
            var userResult = await _userService.AddRoleToUserAsync(
                establishmentUser.UserId, 
                UserRole.EstablishmentMember);

            if (userResult.IsFailed)
                return Result.Fail(userResult.Errors);
        }

        return new CreateEstablishmentUserResponse(establishmentUser.UserId, establishmentUser.EstablishmentId, establishmentUser.Role);
    }
}
