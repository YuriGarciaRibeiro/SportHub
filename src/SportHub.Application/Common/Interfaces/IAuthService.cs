namespace Application.Common.Interfaces;

public interface IAuthService
{
    Task<Result<string>> RegisterAsync(string firstName, string lastName, string email, string password);
    Task<Result<string>> LoginAsync(string email, string password);
}
