using Application.Common.Interfaces;
using Application.CQRS;

namespace SportHub.Application.UseCases.Users.GetUsers;

public class GetUsersHandler : IQueryHandler<GetUsersQuery, List<GetUserDto>>
{
    private readonly IUsersRepository _userRepository;

    public GetUsersHandler(IUsersRepository userRepository)
    {
        _userRepository = userRepository;
    }

    
    public async Task<Result<List<GetUserDto>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetAllAsync();
        return Result.Ok(users.Select(u => new GetUserDto
        {
            Id = u.Id,
            Email = u.Email,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Role = u.Role
        }).ToList());
    }
}