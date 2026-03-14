using Application.Common.Interfaces;
using Application.CQRS;
using Domain.Enums;

namespace Application.UseCases.Members.GetMembers;

public class GetMembersHandler : IQueryHandler<GetMembersQuery, List<MemberDto>>
{
    private readonly IUsersRepository _usersRepository;

    public GetMembersHandler(IUsersRepository usersRepository)
    {
        _usersRepository = usersRepository;
    }

    public async Task<Result<List<MemberDto>>> Handle(GetMembersQuery request, CancellationToken cancellationToken)
    {
        var allUsers = await _usersRepository.GetAllAsync();
        
        // Filter only operational members (Staff, Manager, Owner)
        var members = allUsers
            .Where(u => u.Role >= UserRole.Staff && !u.IsDeleted)
            .Select(m => new MemberDto
            {
                Id = m.Id,
                FullName = m.FullName,
                Email = m.Email,
                Role = m.Role.ToString(),
                CreatedAt = m.CreatedAt,
                LastLoginAt = m.LastLoginAt,
                IsActive = m.IsActive
            })
            .OrderByDescending(m => m.CreatedAt)
            .ToList();

        return Result.Ok(members);
    }
}
