using Application.Common.Interfaces;
using Hangfire;

namespace Infrastructure.Jobs;

public class PeakHoursOrchestratorJob(ITenantRepository tenantRepository)
{
    public async Task ExecuteAsync(CancellationToken ct = default)
    {
        var tenantIds = await tenantRepository.GetAllPeakHoursEnabledAsync(ct);

        foreach (var tenantId in tenantIds)
            BackgroundJob.Enqueue<PeakHoursTenantJob>(job => job.ExecuteAsync(tenantId, CancellationToken.None));
    }
}
