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
using Api.Middleware;
using SportHub.Infrastructure.Extensions;


namespace Api.Extensions;

public static class ServiceExtensions
{
    public static WebApplicationBuilder AddServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IJwtService, JwtService>();
        builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IAuthorizationHandler, GlobalRoleHandler>();
        builder.Services.AddScoped<IAuthorizationHandler, SuperAdminHandler>();
        builder.Services.AddScoped<IPasswordService, PasswordService>();
        builder.Services.AddScoped<IReservationService, ReservationService>();
        builder.Services.AddScoped<ICacheService, CacheService>();
        builder.Services.AddScoped<IRealtimeNotificationService, RealtimeNotificationService>();
        builder.Services.AddSignalR();

        // Unit of Work
        builder.Services.AddScoped<IUnitOfWork>(sp =>
            sp.GetRequiredService<ApplicationDbContext>());

        // TimeProvider para testabilidade de datas
        builder.Services.AddSingleton(TimeProvider.System);

        // Tenant
        builder.Services.AddScoped<ITenantContext, TenantContext>();
        builder.Services.AddScoped<ITenantProvisioningService, TenantProvisioningService>();
        builder.Services.AddScoped<ITenantUsersQueryService, TenantUsersQueryService>();

        // Storage (MinIO dev / AWS S3 prod)
        builder.Services.AddStorage(builder.Configuration);

        return builder;
    }

    public static WebApplicationBuilder AddRepositories(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IUsersRepository, UsersRepository>();
        builder.Services.AddScoped<ICourtsRepository, CourtsRepository>();
        builder.Services.AddScoped<ISportsRepository, SportsRepository>();
        builder.Services.AddScoped<IReservationRepository, ReservationRepository>();

        builder.Services.AddScoped<ILocationsRepository, LocationsRepository>();

        // Tenant
        builder.Services.AddScoped<ITenantRepository, TenantRepository>();

        return builder;
    }

    public static WebApplicationBuilder AddCustomExceptionHandler(this WebApplicationBuilder builder)
    {
        builder.Services.AddExceptionHandler<CustomExceptionHandler>();
        builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, CustomAuthorizationMiddlewareResultHandler>();
        builder.Services.AddProblemDetails();
        return builder;
    }

    public static WebApplicationBuilder AddDatabase(this WebApplicationBuilder builder, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsAssembly("SportHub.Infrastructure"));
        });

        return builder;
    }

    public static WebApplication ExecuteMigrations(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.Migrate();
        logger.LogInformation("Database migrado com sucesso.");

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
            // Policies globais (role no tenant)
            options.AddPolicy(PolicyNames.IsStaff, policy =>
                policy.Requirements.Add(new GlobalRoleRequirement(UserRole.Staff)));

            options.AddPolicy(PolicyNames.IsManager, policy =>
                policy.Requirements.Add(new GlobalRoleRequirement(UserRole.Manager)));

            options.AddPolicy(PolicyNames.IsOwner, policy =>
                policy.Requirements.Add(new GlobalRoleRequirement(UserRole.Owner)));

            // SuperAdmin — operador da plataforma
            options.AddPolicy(PolicyNames.IsSuperAdmin, policy =>
                policy.Requirements.Add(new SuperAdminRequirement()));
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
        builder.Services.AddTransient<SuperAdminSeeder>();
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

    public static WebApplicationBuilder AddCaching(this WebApplicationBuilder builder)
    {
        builder.Services.AddRedis(builder.Configuration);

        return builder;
    }

    public static WebApplicationBuilder AddCors(this WebApplicationBuilder builder)
    {
        var origins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? [];

        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.SetIsOriginAllowed(origin =>
                {
                    // Permitir origens configuradas explicitamente
                    if (origins.Contains(origin)) return true;

                    // Permitir qualquer subdomínio de localhost em dev
                    var host = new Uri(origin).Host;
                    if (host == "localhost" || host.EndsWith(".localhost")) return true;

                    return false;
                })
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
            });
        });

        return builder;
    }
}
