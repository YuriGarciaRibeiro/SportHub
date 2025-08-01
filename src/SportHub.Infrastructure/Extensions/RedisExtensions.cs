using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.StackExchangeRedis;

namespace SportHub.Infrastructure.Extensions;

public static class RedisExtensions
{
    public static IServiceCollection AddRedis(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptionsWithValidateOnStart<RedisOptions>()
            .Bind(configuration.GetRequiredSection(RedisOptions.SectionName))
            .ValidateDataAnnotations();

        var redisOptions = configuration
            .GetRequiredSection(RedisOptions.SectionName)
            .Get<RedisOptions>();

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisOptions.ConnectionString;
            options.InstanceName = $"{redisOptions.InstanceName}:";
        });

        return services;
    }
}

public class RedisOptions
{
    public const string SectionName = "Redis";

    [Required]
    public string ConnectionString { get; set; } = string.Empty;

    [Required]
    public string InstanceName { get; set; } = string.Empty;
}
