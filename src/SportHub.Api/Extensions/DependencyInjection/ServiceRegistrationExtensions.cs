using Application.Services;
using Infrastructure.Repositories;
using Infrastructure.Security;
using Infrastructure.Services;
using Application.Common.Interfaces.Favorites;
using Application.Common.Interfaces.Geography;

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

        return builder;
    }

    public static WebApplicationBuilder AddInfrastructureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IJwtService, JwtService>();
        builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
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
        builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
        builder.Services.AddScoped<IEvaluationRepository, EvaluationRepository>();
        builder.Services.AddScoped<IFavoriteRepository, FavoriteRepository>();
        builder.Services.AddScoped<IFavoriteRepository, FavoriteRepository>();

        return builder;
    }
}
