namespace Application.Common.Interfaces;

public interface IPeakHoursCalculator
{
    Task CalculateAndSetPeakHoursAsync(CancellationToken ct = default);
    Task CalculateAndSetPeakHoursAsync(Guid tenantId, CancellationToken ct = default);
}
