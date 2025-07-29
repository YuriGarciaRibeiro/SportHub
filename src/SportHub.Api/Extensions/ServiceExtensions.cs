using System.Security.Claims;
using System.Text;
using Api.Behaviors;
using Application.Behaviors;
using Application.Common.Interfaces;
using Application.Security;
using Application.Services;
using Application.Settings;
using Application.UseCases.Auth.Register;
using Domain.Enums;
using FluentValidation;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Infrastructure.Security;
using Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using SportHub.Api.Middleware;
using WebAPI.Middleware;


namespace WebAPI.Extensions;

public static class ServiceExtensions
{
    public static WebApplicationBuilder AddServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IJwtService, JwtService>();
        builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IEstablishmentService, EstablishmentService>();
        builder.Services.AddScoped<IEstablishmentRoleService, EstablishmentRoleService>();
        builder.Services.AddScoped<IAuthorizationHandler, EstablishmentHandler>();
        builder.Services.AddScoped<IAuthorizationHandler, GlobalRoleHandler>();
        builder.Services.AddScoped<IPasswordService, PasswordService>();
        return builder;
    }

    public static WebApplicationBuilder AddRepositories(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IEstablishmentsRepository, EstablishmentsRepository>();
        builder.Services.AddScoped<IEstablishmentUsersRepository, EstablishmentUsersRepository>();
        builder.Services.AddScoped<IUsersRepository, UsersRepository>();
        builder.Services.AddScoped<ICourtsRepository, CourtsRepository>();
        builder.Services.AddScoped<ISportsRepository, SportsRepository>();

        return builder;
    }

    public static WebApplicationBuilder AddCustomExecptionHanlder(this WebApplicationBuilder builder)
    {
        builder.Services.AddExceptionHandler<CustomExecptionHandler>();
        builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, CustomAuthorizationMiddlewareResultHandler>();
        builder.Services.AddProblemDetails();
        return builder;
    }

    public static WebApplicationBuilder AddDatabase(this WebApplicationBuilder builder, IConfiguration configuration)
    {
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
        });

        return builder;
    }



    public static WebApplication ExecuteMigrations(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.Migrate();

        return app;
    }


    public static WebApplicationBuilder AddMediatR(this WebApplicationBuilder builder)
    {
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(RegisterUserCommand).Assembly));
        builder.Services.AddValidatorsFromAssemblyContaining<RegisterUserValidator>();
        builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

        return builder;
    }

    public static WebApplicationBuilder AddAuthentication(this WebApplicationBuilder builder)
    {
        var jwtSettings = builder.Configuration.GetSection("Jwt");
        builder.Services.Configure<JwtSettings>(jwtSettings);

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!)),
                NameClaimType = ClaimTypes.NameIdentifier,
                RoleClaimType = ClaimTypes.Role
            };

            
        });
        builder.Services.AddAuthorization(options =>
        {
            // Policies globais (verifica se tem o cargo em qualquer estabelecimento)
            options.AddPolicy(PolicyNames.IsStaff, policy =>
                policy.Requirements.Add(new GlobalRoleRequirement(EstablishmentRole.Staff)));
            
            options.AddPolicy(PolicyNames.IsManager, policy =>
                policy.Requirements.Add(new GlobalRoleRequirement(EstablishmentRole.Manager)));

            options.AddPolicy(PolicyNames.IsOwner, policy =>
                policy.Requirements.Add(new GlobalRoleRequirement(EstablishmentRole.Owner)));

            // Policies específicas do estabelecimento (verifica cargo no estabelecimento específico)
            options.AddPolicy(PolicyNames.IsEstablishmentStaff, policy =>
                policy.Requirements.Add(new EstablishmentRequirement(EstablishmentRole.Staff)));

            options.AddPolicy(PolicyNames.IsEstablishmentManager, policy =>
                policy.Requirements.Add(new EstablishmentRequirement(EstablishmentRole.Manager)));

            options.AddPolicy(PolicyNames.IsEstablishmentOwner, policy =>
                policy.Requirements.Add(new EstablishmentRequirement(EstablishmentRole.Owner)));
        });

        return builder;
    }
    public static WebApplicationBuilder AddSettings(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
        builder.Services.Configure<AdminUserSettings>(builder.Configuration.GetSection("AdminUser"));

        return builder;
    }

    public static WebApplicationBuilder AddSeeders(this WebApplicationBuilder builder)
    {
        builder.Services.AddTransient<CustomUserSeeder>();
        builder.Services.AddTransient<SportSeeder>();
        return builder;
    }

    public static WebApplicationBuilder AddSerilogLogging(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .CreateLogger();

        builder.Host.UseSerilog();
        return builder;
    }
}
