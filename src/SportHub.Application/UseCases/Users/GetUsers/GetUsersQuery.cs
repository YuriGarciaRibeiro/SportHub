using Application.Common.Models;
using Application.CQRS;
using Domain.Enums;

namespace SportHub.Application.UseCases.Users.GetUsers;

public record GetUsersQuery(GetUsersFilter Filter) : IQuery<PagedResult<GetUserDto>>;

public class GetUsersFilter
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public UserRole? Role { get; set; }
    public bool? IsActive { get; set; }
    public string? SearchTerm { get; set; }
}

public class GetUserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public UserRole Role { get; set; }
    public string FullName => $"{FirstName} {LastName}";
}