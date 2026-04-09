using Application.CQRS;

namespace Application.UseCases.Auth.ForgotPassword;

public record ForgotPasswordCommand(string Email) : ICommand;
