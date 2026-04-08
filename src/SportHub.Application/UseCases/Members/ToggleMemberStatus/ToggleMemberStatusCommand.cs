using Application.CQRS;

namespace Application.UseCases.Members.ToggleMemberStatus;

public record ToggleMemberStatusCommand(Guid MemberId) : ICommand<ToggleMemberStatusResponse>;

public record ToggleMemberStatusResponse(Guid Id, string FullName, bool IsActive);
