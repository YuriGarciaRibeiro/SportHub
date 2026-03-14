using Domain.Enums;

namespace SportHub.Tests.Domain.Enums;

public class UserRoleTests
{
    [Fact]
    public void UserRole_ShouldHaveCorrectValues()
    {
        Assert.Equal(0, (int)UserRole.Customer);
        Assert.Equal(1, (int)UserRole.Staff);
        Assert.Equal(2, (int)UserRole.Manager);
        Assert.Equal(3, (int)UserRole.Owner);
        Assert.Equal(99, (int)UserRole.SuperAdmin);
    }

    [Fact]
    public void UserRole_ShouldSupportHierarchicalComparison()
    {
        Assert.True(UserRole.Customer < UserRole.Staff);
        Assert.True(UserRole.Staff < UserRole.Manager);
        Assert.True(UserRole.Manager < UserRole.Owner);
        Assert.True(UserRole.Owner < UserRole.SuperAdmin);
    }

    [Fact]
    public void UserRole_ShouldHaveExactlyFiveValues()
    {
        var values = Enum.GetValues<UserRole>();
        Assert.Equal(5, values.Length);
    }

    [Fact]
    public void EstablishmentRole_ShouldNotExist()
    {
        var assembly = typeof(UserRole).Assembly;
        var establishmentRoleType = assembly.GetTypes()
            .FirstOrDefault(t => t.Name == "EstablishmentRole");
        
        Assert.Null(establishmentRoleType);
    }

    [Theory]
    [InlineData(UserRole.Customer, "Customer")]
    [InlineData(UserRole.Staff, "Staff")]
    [InlineData(UserRole.Manager, "Manager")]
    [InlineData(UserRole.Owner, "Owner")]
    [InlineData(UserRole.SuperAdmin, "SuperAdmin")]
    public void UserRole_ToRoleName_ShouldReturnCorrectString(UserRole role, string expected)
    {
        var result = role.ToRoleName();
        Assert.Equal(expected, result);
    }
}
