using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.Extensions.Configuration;

namespace SportHub.Infrastructure.Extensions;

public static class HangfireExtensions
{
    public static IGlobalConfiguration UsePostgresStorage(
        this IGlobalConfiguration config,
        IConfiguration configuration)
    {
        var hangfire = configuration.GetConnectionString("HangfireConnection");
        var connectionString = string.IsNullOrWhiteSpace(hangfire)
            ? configuration.GetConnectionString("DefaultConnection")
            : hangfire;

        return config.UsePostgreSqlStorage(options =>
            options.UseNpgsqlConnection(connectionString));
    }
}
