using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.CQRS;

namespace Application.UseCases.Auth.GetCurrentUser;

public class GetCurrentUserHandler : IQueryHandler<GetCurrentUserQuery, UserProfileResponse>
{
    private readonly IUsersRepository _usersRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IReservationRepository _reservationRepository;

    public GetCurrentUserHandler(
        IUsersRepository usersRepository,
        ICurrentUserService currentUserService,
        IReservationRepository reservationRepository)
    {
        _usersRepository = usersRepository;
        _currentUserService = currentUserService;
        _reservationRepository = reservationRepository;
    }

    public async Task<Result<UserProfileResponse>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _usersRepository.GetByIdAsync(_currentUserService.UserId);

        if (user is null)
            return Result.Fail(new NotFound("Usuário não encontrado."));

        var totalSpent = await _reservationRepository.GetTotalSpentByUserAsync(
            _currentUserService.UserId, cancellationToken);

        return Result.Ok(new UserProfileResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            $"{user.FirstName} {user.LastName}".Trim(),
            user.Email,
            user.Role.ToString(),
            user.LastLoginAt,
            user.CreatedAt,
            totalSpent
        ));
    }
}
