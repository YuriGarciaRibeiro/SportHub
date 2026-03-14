namespace Domain.Enums;

public enum UserRole
{
    User = 0,
    EstablishmentMember = 1,
    Admin = 2,
    SuperAdmin = 99
}

public static class UserRoleExtensions
{
    public static string ToRoleName(this UserRole role)
        => role.ToString();
}