using Application.Common.Interfaces;

namespace Infrastructure.Jobs;

public class PeakHoursTenantJob(IPeakHoursCalculator calculator)
{
    public async Task ExecuteAsync(Guid tenantId, CancellationToken ct = default)
        => await calculator.CalculateAndSetPeakHoursAsync(tenantId, ct);
}
