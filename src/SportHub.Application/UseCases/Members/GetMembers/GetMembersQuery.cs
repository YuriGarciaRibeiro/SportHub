using Application.CQRS;

namespace Application.UseCases.Members.GetMembers;

public record GetMembersQuery : IQuery<List<MemberDto>>;
