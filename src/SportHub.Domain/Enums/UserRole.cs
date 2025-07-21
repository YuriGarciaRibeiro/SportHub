namespace Domain.Enums;

public enum UserRole
{
    Admin,
    EstablishmentMember,
    User
}

public static class UserRoleExtensions
{
    public static string ToRoleName(this UserRole role)
        => role.ToString();
}