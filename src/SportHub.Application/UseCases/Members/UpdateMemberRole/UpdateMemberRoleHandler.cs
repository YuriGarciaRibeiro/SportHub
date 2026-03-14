using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.CQRS;
using Domain.Enums;

namespace Application.UseCases.Members.UpdateMemberRole;

public class UpdateMemberRoleHandler : ICommandHandler<UpdateMemberRoleCommand, MemberRoleUpdatedResponse>
{
    private readonly IUsersRepository _usersRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateMemberRoleHandler(
        IUsersRepository usersRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _usersRepository = usersRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<MemberRoleUpdatedResponse>> Handle(UpdateMemberRoleCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        var currentUserRole = _currentUserService.UserRole;

        // Only Owner can update member roles
        if (currentUserRole != UserRole.Owner)
            return Result.Fail(new Forbidden("Only Owner can update member roles"));

        // Get the member to update
        var member = await _usersRepository.GetByIdAsync(request.MemberId);
        if (member is null)
            return Result.Fail(new NotFound("Member not found"));

        // Owner cannot change their own role
        if (member.Id == currentUserId)
            return Result.Fail(new BadRequest("You cannot change your own role"));

        // Cannot promote to Owner (only one Owner per tenant)
        if (request.NewRole == UserRole.Owner)
            return Result.Fail(new BadRequest("Cannot promote to Owner. Only one Owner per tenant is allowed"));

        // Cannot assign SuperAdmin
        if (request.NewRole == UserRole.SuperAdmin)
            return Result.Fail(new BadRequest("Cannot assign SuperAdmin role"));

        // Update the role
        member.Role = request.NewRole;
        await _usersRepository.UpdateAsync(member);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok(new MemberRoleUpdatedResponse(
            member.Id,
            member.FullName,
            member.Email,
            member.Role.ToString()
        ));
    }
}
