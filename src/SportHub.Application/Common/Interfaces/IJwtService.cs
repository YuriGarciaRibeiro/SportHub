namespace Application.Common.Interfaces;

public interface IJwtService
{
    (string Token, DateTime ExpiresAt) GenerateToken(Guid userId, string fullName, string email);
}
