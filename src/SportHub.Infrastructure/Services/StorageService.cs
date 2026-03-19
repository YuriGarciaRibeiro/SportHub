using Amazon.S3;
using Amazon.S3.Model;
using Application.Common.Interfaces;
using Application.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public class StorageService : IStorageService
{
    private readonly IAmazonS3 _s3;
    private readonly StorageSettings _settings;
    private readonly ILogger<StorageService> _logger;

    public StorageService(IAmazonS3 s3, IOptions<StorageSettings> options, ILogger<StorageService> logger)
    {
        _s3 = s3;
        _settings = options.Value;
        _logger = logger;
    }

    public async Task<string> UploadAsync(
        string key,
        Stream stream,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var request = new PutObjectRequest
        {
            BucketName = _settings.BucketName,
            Key = key,
            InputStream = stream,
            ContentType = contentType,
        };

        _logger.LogInformation("Uploading object {Key} to bucket {Bucket}", key, _settings.BucketName);
        await _s3.PutObjectAsync(request, cancellationToken);

        return $"{_settings.PublicBaseUrl.TrimEnd('/')}/{key}";
    }

    public async Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        var request = new DeleteObjectRequest
        {
            BucketName = _settings.BucketName,
            Key = key,
        };

        _logger.LogInformation("Deleting object {Key} from bucket {Bucket}", key, _settings.BucketName);
        await _s3.DeleteObjectAsync(request, cancellationToken);
    }
}
