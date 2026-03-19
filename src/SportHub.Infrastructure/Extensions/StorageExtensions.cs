using Amazon;
using Amazon.S3;
using Application.Common.Interfaces;
using Application.Settings;
using Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SportHub.Infrastructure.Extensions;

public static class StorageExtensions
{
    public static IServiceCollection AddStorage(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var settings = configuration.GetSection("Storage").Get<StorageSettings>()
            ?? throw new InvalidOperationException("Configuração 'Storage' não encontrada.");

        services.Configure<StorageSettings>(configuration.GetSection("Storage"));

        var s3Config = new AmazonS3Config
        {
            RegionEndpoint = RegionEndpoint.GetBySystemName(settings.Region)
        };

        if (!string.IsNullOrWhiteSpace(settings.ServiceUrl))
        {
            s3Config.ServiceURL = settings.ServiceUrl;
            s3Config.ForcePathStyle = true; // obrigatório para MinIO
        }

        services.AddSingleton<IAmazonS3>(new AmazonS3Client(
            settings.AccessKey,
            settings.SecretKey,
            s3Config));

        services.AddScoped<IStorageService, StorageService>();

        return services;
    }
}
