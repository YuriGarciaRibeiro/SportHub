namespace Application.Common.Interfaces.Email;

public interface ICustomEmailSender
{
    Task SendAsync(string to, string subject, string body, CancellationToken ct = default);
}