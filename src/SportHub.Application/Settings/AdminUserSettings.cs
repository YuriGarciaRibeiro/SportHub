namespace Application.Settings;

public class AdminUserSettings
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string FirstName { get; set; } = "Admin";
    public string LastName { get; set; } = "User";
}