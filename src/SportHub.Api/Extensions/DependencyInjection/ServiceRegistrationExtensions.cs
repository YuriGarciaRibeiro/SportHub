using Application.Services;
using Infrastructure.Repositories;
using Infrastructure.Security;
using Infrastructure.Services;
using Application.Common.Interfaces.Favorites;
using Application.Common.Interfaces.Geography;
using SportHub.Application.Common.Interfaces.PasswordReset;
using SportHub.Application.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using SportHub.Infrastructure.Repositories;
using Application.Common.Interfaces.Email;
using SportHub.Application.Options;
using SportHub.Application.Common.Interfaces.Email;

namespace Api.Extensions.DependencyInjection;

public static class ServiceRegistrationExtensions
{
    public static WebApplicationBuilder AddApplicationServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IEstablishmentService, EstablishmentService>();
        builder.Services.AddScoped<IEstablishmentRoleService, EstablishmentRoleService>();
        builder.Services.AddScoped<IEstablishmentUserService, EstablishmentUserService>();
        builder.Services.AddScoped<ICourtService, CourtService>();
        builder.Services.AddScoped<ISportService, SportService>();
        builder.Services.AddScoped<IEvaluationService, EvaluationService>();
        builder.Services.AddScoped<IReservationService, ReservationService>();
        builder.Services.AddScoped<ICacheService, CacheService>();
        builder.Services.AddScoped<IFavoriteService, FavoriteService>();
        builder.Services.AddScoped<IGeographyService, GeographyService>();
        builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();
        builder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();

        return builder;
    }

    public static WebApplicationBuilder AddInfrastructureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IJwtService, JwtService>();
        builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
        builder.Services.AddScoped<IPasswordService, PasswordService>();
        builder.Services.AddScoped<ICustomEmailSender, SmtpEmailSender>();
        builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));
        return builder;
    }

    public static WebApplicationBuilder AddRepositories(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IEstablishmentsRepository, EstablishmentsRepository>();
        builder.Services.AddScoped<IEstablishmentUsersRepository, EstablishmentUsersRepository>();
        builder.Services.AddScoped<IUsersRepository, UsersRepository>();
        builder.Services.AddScoped<ICourtsRepository, CourtsRepository>();
        builder.Services.AddScoped<ISportsRepository, SportsRepository>();
        builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
        builder.Services.AddScoped<IEvaluationRepository, EvaluationRepository>();
        builder.Services.AddScoped<IFavoriteRepository, FavoriteRepository>();
        builder.Services.AddScoped<IFavoriteRepository, FavoriteRepository>();
        builder.Services.AddScoped<IOtpCodeRepository, OtpCodeRepository>();
        builder.Services.AddScoped<IResetSessionRepository, ResetSessionRepository>();

        return builder;
    }
}
