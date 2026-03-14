namespace Domain.Enums;

public enum UserRole
{
    Customer = 0,
    Staff = 1,
    Manager = 2,
    Owner = 3,
    SuperAdmin = 99
}

public static class UserRoleExtensions
{
    public static string ToRoleName(this UserRole role)
        => role.ToString();
}