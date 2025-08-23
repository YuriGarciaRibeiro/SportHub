using Api.Behaviors;
using Api.Middleware;
using Application.Behaviors;
using Application.UseCases.Auth.Register;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace Api.Extensions.MediatR;

public static class MediatRExtensions
{
    public static WebApplicationBuilder AddMediatR(this WebApplicationBuilder builder)
    {
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(RegisterUserCommand).Assembly));
        builder.Services.AddValidatorsFromAssemblyContaining<RegisterUserValidator>();
        builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

        return builder;
    }

    public static WebApplicationBuilder AddCustomExceptionHandler(this WebApplicationBuilder builder)
    {
        builder.Services.AddExceptionHandler<CustomExceptionHandler>();
        builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, CustomAuthorizationMiddlewareResultHandler>();
        builder.Services.AddProblemDetails();
        return builder;
    }
}
