namespace Application.Common.Interfaces;

public interface IAuthService
{
    Task<Result<string>> RegisterAsync(string fullName, string email, string password);
}
