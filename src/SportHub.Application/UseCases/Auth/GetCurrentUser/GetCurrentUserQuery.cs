using Application.CQRS;

namespace Application.UseCases.Auth.GetCurrentUser;

public record GetCurrentUserQuery : IQuery<UserProfileResponse>;

public record UserProfileResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    string Role,
    DateTime? LastLoginAt,
    DateTime CreatedAt,
    decimal TotalSpent
);
