using Application.Common.Interfaces;
using Application.Common.Models;
using Application.CQRS;
using Domain.Enums;

namespace Application.UseCases.Members.GetMembers;

public class GetMembersHandler : IQueryHandler<GetMembersQuery, PagedResult<MemberDto>>
{
    private readonly IUsersRepository _usersRepository;

    public GetMembersHandler(IUsersRepository usersRepository)
    {
        _usersRepository = usersRepository;
    }

    public async Task<Result<PagedResult<MemberDto>>> Handle(GetMembersQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;

        // Only allow filtering by Staff+ roles — Customer/SuperAdmin are not valid here
        var role = filter.Role.HasValue && filter.Role.Value >= UserRole.Staff && filter.Role.Value < UserRole.SuperAdmin
            ? filter.Role
            : null;

        var memberRoles = new[] { UserRole.Staff, UserRole.Manager, UserRole.Owner };
        var allowedRoles = role.HasValue ? new[] { role.Value } : memberRoles;

        var pagedUsers = await _usersRepository.GetPagedAsync(
            page: filter.Page ?? 1,
            pageSize: filter.PageSize ?? 10,
            isActive: filter.IsActive,
            searchTerm: filter.SearchTerm,
            allowedRoles: allowedRoles);

        var items = pagedUsers.Items
            .Select(u => new MemberDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role.ToString(),
                CreatedAt = u.CreatedAt,
                LastLoginAt = u.LastLoginAt,
                IsActive = u.IsActive
            })
            .ToList();

        var result = new PagedResult<MemberDto>
        {
            Items = items,
            TotalCount = pagedUsers.TotalCount,
            Page = pagedUsers.Page,
            PageSize = pagedUsers.PageSize
        };

        return Result.Ok(result);
    }
}
