using Application.UserCases.Auth;

namespace Application.Common.Interfaces;

public interface IAuthService
{
    Task<Result<AuthResponse>> RegisterAsync(string firstName, string lastName, string email, string password);
    Task<Result<AuthResponse>> LoginAsync(string email, string password);
}
