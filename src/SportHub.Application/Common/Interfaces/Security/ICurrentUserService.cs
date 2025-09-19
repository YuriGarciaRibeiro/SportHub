namespace Application.Common.Interfaces.Security;

public interface ICurrentUserService
{
    Guid UserId { get; }
    string FullName { get; }
    string Email { get; }
    bool IsAuthenticated { get; }
}
