namespace Domain.Enums;

public enum UserRole
{
    User,
    Admin,
    Coach
}

public static class UserRoleExtensions
{
    public static string ToRoleName(this UserRole role)
        => role.ToString();
}