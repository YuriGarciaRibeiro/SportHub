namespace Application.Common.Interfaces;

public interface IPasswordService
{
    string HashPassword(string password, out string salt);
    bool VerifyPassword(string password, string hash, string salt);
    string GenerateSalt();
}
