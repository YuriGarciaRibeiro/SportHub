using Application.CQRS;
using Application.UseCases.Members.GetMembers;
using Domain.Enums;

namespace Application.UseCases.Members.UpsertMember;

public record UpsertMemberCommand(
    string Email,
    string FirstName,
    string LastName,
    UserRole Role
) : ICommand<MemberDto>;
