namespace SportHub.Application.Services;

using global::Application.Common.Interfaces.Email;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using SportHub.Application.Options;

public class SmtpEmailSender : ICustomEmailSender
{
    private readonly SmtpOptions _opt;

    public SmtpEmailSender(IOptions<SmtpOptions> opt) => _opt = opt.Value;

    public async Task SendAsync(string to, string subject, string body, CancellationToken ct = default)
    {
        var msg = new MimeMessage();
        msg.From.Add(new MailboxAddress(_opt.FromName, _opt.FromAddress));
        msg.To.Add(MailboxAddress.Parse(to));
        msg.Subject = subject;

        var builder = new BodyBuilder();
        if (_opt.HtmlBody)
            builder.HtmlBody = body;
        else
            builder.TextBody = body;

        msg.Body = builder.ToMessageBody();

        using var client = new SmtpClient();
        // conecta
        var secure = _opt.UseStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto;
        await client.ConnectAsync(_opt.Host, _opt.Port, secure, ct);

        if (!string.IsNullOrWhiteSpace(_opt.User))
            await client.AuthenticateAsync(_opt.User, _opt.Password, ct);

        await client.SendAsync(msg, ct);
        await client.DisconnectAsync(true, ct);
    }
}
