using Application.CQRS;
using Domain.Enums;

namespace Application.UseCases.Members.UpdateMemberRole;

public record UpdateMemberRoleCommand(Guid MemberId, UserRole NewRole) : ICommand<MemberRoleUpdatedResponse>;

public record MemberRoleUpdatedResponse(Guid Id, string FullName, string Email, string Role);
