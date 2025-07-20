    using System.Security.Claims;
    using Application.Common.Interfaces;
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
                var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                return Guid.TryParse(userId, out var guid) ? guid : Guid.Empty;
            }
        }
}
