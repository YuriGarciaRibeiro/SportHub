using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.CQRS;
using Domain.Entities;
using Domain.Enums;
using FluentResults;

namespace Application.UserCases.EstablishmentUser.CreateEstablishmentUser;

public class CreateEstablishmentUserHandler : ICommandHandler<CreateEstablishmentUserCommand, CreateEstablishmentUserResponse>
{
    private readonly IEstablishmentUsersRepository _repository;
    private readonly IUserService _userService;


    public CreateEstablishmentUserHandler(IEstablishmentUsersRepository repository, IUserService userService)
    {
        _repository = repository;
        _userService = userService;
    }

    public async Task<Result<CreateEstablishmentUserResponse>> Handle(CreateEstablishmentUserCommand request, CancellationToken cancellationToken)
    {
        // Verificar se o usuário existe
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

        // Se o usuário for apenas "User", atualize para "EstablishmentMember"
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
