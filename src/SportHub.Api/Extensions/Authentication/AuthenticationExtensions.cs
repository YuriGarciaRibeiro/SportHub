using System.Security.Claims;
using System.Text;
using Application.Security;
using Application.Settings;
using Domain.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Infrastructure.Security;

namespace Api.Extensions.Authentication;

public static class AuthenticationExtensions
{
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

        return builder;
    }

    public static WebApplicationBuilder AddAuthorization(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IAuthorizationHandler, EstablishmentHandler>();
        builder.Services.AddScoped<IAuthorizationHandler, GlobalRoleHandler>();
        builder.Services.AddScoped<IAuthorizationHandler, ReservationOwnerHandler>();

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

            options.AddPolicy(PolicyNames.IsReservationOwner, policy =>
                policy.Requirements.Add(new ReservationOwnerRequirement()));
        });

        return builder;
    }
}
