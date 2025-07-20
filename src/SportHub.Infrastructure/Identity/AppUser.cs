using Domain.Entities;
using Microsoft.AspNetCore.Identity;


namespace Infrastructure.Identity;

public class AppUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;

    public string FullName => $"{FirstName} {LastName}";
    
    public User ToDomain()
    {
        return new User
        {
            Id = Id,
            Email = Email!,
            FirstName = FirstName,
            LastName = LastName
        };
    }

    public static AppUser FromDomain(User user)
    {
        return new AppUser
        {
            Id = user.Id,
            Email = user.Email,
            UserName = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName
        };
    }

}
