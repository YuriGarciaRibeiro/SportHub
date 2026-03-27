using Hangfire;
using Microsoft.AspNetCore.Builder;

namespace Infrastructure.Jobs;

public static class JobsExtensions
{
    public static WebApplication UseRecurringJobs(this WebApplication app)
    {
        RecurringJob.AddOrUpdate<PeakHoursOrchestratorJob>(
            "peak-hours-orchestrator",
            job => job.ExecuteAsync(CancellationToken.None),
            Cron.Weekly(DayOfWeek.Sunday, 3, 0)); // Executa toda segunda-feira às 3h da manhã

        return app;
    }
}
