namespace Application.Common.Interfaces;

public interface IJwtService
{
    string GenerateToken(Guid userId, string fullName, string email);
}
