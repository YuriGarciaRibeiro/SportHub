namespace Application.Emails.welcome;

public sealed record WelcomeEmailModel(
        string UserName,
        string UserEmail,
        string AppUrl,
        string VerifyEmailUrl,
        string SupportEmail,
        string SupportUrl,
        int Year);