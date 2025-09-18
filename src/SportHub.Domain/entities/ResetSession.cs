namespace Domain.Entities;

public class ResetSession
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N"); // GUID curto
    public Guid UserId { get; set; }
    public string Purpose { get; set; } = "password_reset";
    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }
}