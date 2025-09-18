namespace SportHub.Application.Options;

public class SmtpOptions
{
    public string Host { get; set; } = null!;
    public int Port { get; set; } = 587;
    public bool UseStartTls { get; set; } = true; // STARTTLS
    public string User { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string FromAddress { get; set; } = null!;
    public string FromName { get; set; } = "No-Reply";
    public bool HtmlBody { get; set; } = false; // enviar como texto puro por padrão
}
