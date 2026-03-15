using Application.Common.Interfaces;
using Application.Common.Models;
using Application.CQRS;

namespace SportHub.Application.UseCases.Users.GetUsers;

public class GetUsersHandler : IQueryHandler<GetUsersQuery, PagedResult<GetUserDto>>
{
    private readonly IUsersRepository _userRepository;

    public GetUsersHandler(IUsersRepository userRepository)
    {
        _userRepository = userRepository;
    }

    
    public async Task<Result<PagedResult<GetUserDto>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;
        
        var pagedUsers = await _userRepository.GetPagedAsync(
            page: filter.Page,
            pageSize: filter.PageSize,
            email: filter.Email,
            firstName: filter.FirstName,
            lastName: filter.LastName,
            role: filter.Role,
            isActive: filter.IsActive,
            searchTerm: filter.SearchTerm);

        var result = new PagedResult<GetUserDto>
        {
            Items = pagedUsers.Items.Select(u => new GetUserDto
            {
                Id = u.Id,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Role = u.Role
            }).ToList(),
            TotalCount = pagedUsers.TotalCount,
            Page = pagedUsers.Page,
            PageSize = pagedUsers.PageSize
        };

        return Result.Ok(result);
    }
}