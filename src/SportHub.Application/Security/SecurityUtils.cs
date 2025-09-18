namespace SportHub.Application.Security;

// Application/Security/SecurityUtils.cs
using System.Security.Cryptography;
using System.Text;

public static class SecurityUtils
{
    public static string GenerateSixDigitCode()
    {
        Span<byte> b = stackalloc byte[4];
        RandomNumberGenerator.Fill(b);
        uint num = BitConverter.ToUInt32(b) % 1_000_000u; // 0..999999
        return num.ToString("D6");
    }

    public static string Sha256Hex(string s)
    {
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(s));
        return Convert.ToHexString(hash); // "A1B2..."
    }

    // Armazene em Base64 para casar com seu User (strings)
    public static (string HashB64, string SaltB64, int Iterations) HashPassword(string password, int iterations = 100_000)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
        byte[] hash = pbkdf2.GetBytes(32);
        return (Convert.ToBase64String(hash), Convert.ToBase64String(salt), iterations);
    }

    public static bool VerifyPassword(string password, string hashB64, string saltB64, int iterations)
    {
        var salt = Convert.FromBase64String(saltB64);
        var expected = Convert.FromBase64String(hashB64);
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
        var test = pbkdf2.GetBytes(32);
        return CryptographicOperations.FixedTimeEquals(test, expected);
    }
}
