namespace Application.Common.Interfaces.Security;

public interface IJwtService
{
    (string Token, DateTime ExpiresAt) GenerateToken(Guid userId, string fullName,string role, string email);
}
