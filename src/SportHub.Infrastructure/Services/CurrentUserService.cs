using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var guid) ? guid : Guid.Empty;
        }
    }

    public string FullName
    {
        get
        {
            var fullNameClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name);
            return fullNameClaim?.Value ?? string.Empty;
        }
    }

    public string Email
    {
        get
        {
            var emailClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email);
            return emailClaim?.Value ?? string.Empty;
        }
    }
    public bool IsAuthenticated => UserId != Guid.Empty;
}
