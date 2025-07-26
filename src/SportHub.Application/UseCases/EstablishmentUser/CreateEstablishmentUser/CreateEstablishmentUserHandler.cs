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

        var users = await _userService.GetByIdsAsync(request.Users.Select(u => u.UserId));

        var establishmentUsers = request.Users.Select(user => new Domain.Entities.EstablishmentUser
        {
            UserId = user.UserId,
            EstablishmentId = request.EstablishmentId,
            Role = user.Role
        }).ToList();

        await _repository.AddManyAsync(establishmentUsers);

        foreach( var user in users.Value)
        {
            if (user.Role == UserRole.User )
            {
                var roleResult = await _userService.AddRoleToUserAsync(user.Id, UserRole.EstablishmentMember);
                if (roleResult.IsFailed)
                {
                    return Result.Fail(roleResult.Errors);
                }
            }
        }

        var responseUsers = establishmentUsers.Select(eu => new EstablishmentUserResponse(eu.UserId, eu.Role));

        return new CreateEstablishmentUserResponse(
            Users: responseUsers,
            EstablishmentId: request.EstablishmentId
        );
    }
}
