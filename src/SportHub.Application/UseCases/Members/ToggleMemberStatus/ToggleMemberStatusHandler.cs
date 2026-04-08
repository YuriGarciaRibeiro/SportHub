using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.CQRS;
using Domain.Enums;

namespace Application.UseCases.Members.ToggleMemberStatus;

public class ToggleMemberStatusHandler : ICommandHandler<ToggleMemberStatusCommand, ToggleMemberStatusResponse>
{
    private readonly IUsersRepository _usersRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public ToggleMemberStatusHandler(
        IUsersRepository usersRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _usersRepository = usersRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ToggleMemberStatusResponse>> Handle(ToggleMemberStatusCommand request, CancellationToken cancellationToken)
    {
        var member = await _usersRepository.GetByIdAsync(request.MemberId);
        if (member is null)
            return Result.Fail(new NotFound("Member not found"));

        if (member.Id == _currentUserService.UserId)
            return Result.Fail(new BadRequest("You cannot change your own status"));

        if (member.Role is UserRole.Owner or UserRole.SuperAdmin)
            return Result.Fail(new BadRequest("Cannot change the status of an Owner or SuperAdmin"));

        member.IsActive = !member.IsActive;
        await _usersRepository.UpdateAsync(member);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok(new ToggleMemberStatusResponse(member.Id, member.FullName, member.IsActive));
    }
}
