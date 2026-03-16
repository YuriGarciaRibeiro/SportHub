using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.CQRS;
using Application.UseCases.Members.GetMembers;
using Domain.Entities;
using Domain.Enums;

namespace Application.UseCases.Members.UpsertMember;

public class UpsertMemberHandler : ICommandHandler<UpsertMemberCommand, MemberDto>
{
    private readonly IUsersRepository _usersRepository;
    private readonly IPasswordService _passwordService;
    private readonly IUnitOfWork _unitOfWork;

    private const string DefaultPassword = "Member@123";

    public UpsertMemberHandler(
        IUsersRepository usersRepository,
        IPasswordService passwordService,
        IUnitOfWork unitOfWork)
    {
        _usersRepository = usersRepository;
        _passwordService = passwordService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<MemberDto>> Handle(UpsertMemberCommand request, CancellationToken cancellationToken)
    {
        if (request.Role is UserRole.Owner or UserRole.SuperAdmin)
            return Result.Fail(new BadRequest("Cannot assign Owner or SuperAdmin role."));

        var existing = await _usersRepository.GetByEmailAsync(request.Email);

        if (existing is not null)
        {
            existing.Role = request.Role;
            await _usersRepository.UpdateAsync(existing);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Ok(ToDto(existing));
        }

        var passwordHash = _passwordService.HashPassword(DefaultPassword, out var salt);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PasswordHash = passwordHash,
            Salt = salt,
            Role = request.Role,
            IsActive = true
        };

        await _usersRepository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok(ToDto(user));
    }

    private static MemberDto ToDto(User user) => new()
    {
        Id = user.Id,
        FullName = user.FullName,
        Email = user.Email,
        Role = user.Role.ToString(),
        CreatedAt = user.CreatedAt,
        LastLoginAt = user.LastLoginAt,
        IsActive = user.IsActive
    };
}
