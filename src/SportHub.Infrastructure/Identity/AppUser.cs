using Microsoft.AspNetCore.Identity;
using System;

namespace Infrastructure.Identity;

public class AppUser : IdentityUser<Guid>
{
    public string FullName { get; set; }
}
