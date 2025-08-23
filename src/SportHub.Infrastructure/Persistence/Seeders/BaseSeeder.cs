using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence.Seeders;

public abstract class BaseSeeder : IDataSeeder
{
    protected readonly ILogger _logger;

    protected BaseSeeder(ILogger logger)
    {
        _logger = logger;
    }

    public abstract Task SeedAsync(CancellationToken cancellationToken = default);
    public abstract int Order { get; }

    protected void LogInfo(string message)
    {
        _logger.LogInformation($"[{GetType().Name}] {message}");
    }

    protected void LogWarning(string message)
    {
        _logger.LogWarning($"[{GetType().Name}] {message}");
    }

    protected void LogError(string message, Exception? exception = null)
    {
        _logger.LogError(exception, $"[{GetType().Name}] {message}");
    }
}
