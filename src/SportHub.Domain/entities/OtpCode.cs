namespace Domain.Entities;

public class OtpCode
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Purpose { get; set; } = "password_reset";
    public string CodeHash { get; set; } = null!; // SHA-256 em HEX/Base64
    public DateTime ExpiresAt { get; set; }
    public int Attempts { get; set; }
    public int MaxAttempts { get; set; } = 5;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UsedAt { get; set; }
}