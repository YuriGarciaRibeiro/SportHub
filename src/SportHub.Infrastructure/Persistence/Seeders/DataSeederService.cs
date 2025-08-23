using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence.Seeders;

public class DataSeederService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DataSeederService> _logger;

    public DataSeederService(IServiceProvider serviceProvider, ILogger<DataSeederService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task SeedAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("=== Starting complete data seeding process ===");

        try
        {
            // Get all seeders and order them
            var seeders = _serviceProvider.GetServices<IDataSeeder>()
                .OrderBy(s => s.Order)
                .ToList();

            _logger.LogInformation($"Found {seeders.Count} seeders to execute");

            foreach (var seeder in seeders)
            {
                _logger.LogInformation($"Executing seeder: {seeder.GetType().Name} (Order: {seeder.Order})");
                
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                await seeder.SeedAsync(cancellationToken);
                stopwatch.Stop();
                
                _logger.LogInformation($"Completed seeder: {seeder.GetType().Name} in {stopwatch.ElapsedMilliseconds}ms");
            }

            _logger.LogInformation("=== Data seeding process completed successfully ===");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during data seeding process");
            throw;
        }
    }

    public async Task SeedSpecificAsync<T>(CancellationToken cancellationToken = default) where T : IDataSeeder
    {
        _logger.LogInformation($"Starting specific seeding for: {typeof(T).Name}");

        try
        {
            var seeder = _serviceProvider.GetRequiredService<T>();
            await seeder.SeedAsync(cancellationToken);
            
            _logger.LogInformation($"Completed specific seeding for: {typeof(T).Name}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error occurred during specific seeding for: {typeof(T).Name}");
            throw;
        }
    }
}
