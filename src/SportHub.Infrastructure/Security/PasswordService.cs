using System.Security.Cryptography;
using System.Text;
using Application.Common.Interfaces;

namespace Infrastructure.Security;

public class PasswordService : IPasswordService
{
    private const int SaltSize = 32;
    private const int HashSize = 64;
    private const int Iterations = 10000;

    public string HashPassword(string password, out string salt)
    {
        salt = GenerateSalt();
        return HashPassword(password, salt);
    }

    public bool VerifyPassword(string password, string hash, string salt)
    {
        var hashToCompare = HashPassword(password, salt);
        return hashToCompare == hash;
    }

    public string GenerateSalt()
    {
        var saltBytes = new byte[SaltSize];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(saltBytes);
        }
        return Convert.ToBase64String(saltBytes);
    }

    private string HashPassword(string password, string salt)
    {
        var saltBytes = Convert.FromBase64String(salt);
        
        using var pbkdf2 = new Rfc2898DeriveBytes(
            Encoding.UTF8.GetBytes(password), 
            saltBytes, 
            Iterations, 
            HashAlgorithmName.SHA256);
        
        var hashBytes = pbkdf2.GetBytes(HashSize);
        return Convert.ToBase64String(hashBytes);
    }
}
