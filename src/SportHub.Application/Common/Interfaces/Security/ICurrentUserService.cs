namespace Application.Common.Interfaces.Security;

public interface ICurrentUserService
{
    Guid UserId { get; }
}
