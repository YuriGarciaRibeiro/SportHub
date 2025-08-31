using Infrastructure.Persistence.Seeders;

namespace Api.Extensions.Configuration;

public static class SeederExtensions
{
    public static WebApplicationBuilder AddSeeders(this WebApplicationBuilder builder)
    {
        // Register all seeders as IDataSeeder
        builder.Services.AddScoped<IDataSeeder, UserSeeder>();
        builder.Services.AddScoped<IDataSeeder, SportSeeder>();
        builder.Services.AddScoped<IDataSeeder, EstablishmentSeeder>();
        builder.Services.AddScoped<IDataSeeder, CourtSeeder>();
        builder.Services.AddScoped<IDataSeeder, ReservationSeeder>();

        // Register individual seeders for specific access
        builder.Services.AddScoped<UserSeeder>();
        builder.Services.AddScoped<SportSeeder>();
        builder.Services.AddScoped<EstablishmentSeeder>();
        builder.Services.AddScoped<CourtSeeder>();
        builder.Services.AddScoped<ReservationSeeder>();

        // Register the seeder service
        builder.Services.AddScoped<DataSeederService>();

        return builder;
    }
}
