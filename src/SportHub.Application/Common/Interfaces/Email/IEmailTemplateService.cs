namespace SportHub.Application.Common.Interfaces.Email;

public interface IEmailTemplateService
{
    Task<string> GetTemplateAsync(string templateName, CancellationToken cancellationToken = default);
    Task<string> RenderTemplateAsync(string templateName, object model, CancellationToken cancellationToken = default);
}