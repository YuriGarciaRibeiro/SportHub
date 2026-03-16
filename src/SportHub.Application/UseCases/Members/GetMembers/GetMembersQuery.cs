using Application.Common.Models;
using Application.CQRS;
using Domain.Enums;

namespace Application.UseCases.Members.GetMembers;

public record GetMembersQuery(GetMembersFilter Filter) : IQuery<PagedResult<MemberDto>>;

public class GetMembersFilter
{
    public int? Page { get; set; }
    public int? PageSize { get; set; }

    public string? SearchTerm { get; set; }
    public UserRole? Role { get; set; }
    public bool? IsActive { get; set; }
}
