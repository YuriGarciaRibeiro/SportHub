using Application.Common.Interfaces;
using Domain.Entities;

namespace Infrastructure.Services;

/// <summary>
/// Implementação Scoped de ITenantContext.
/// Começa não resolvida. O TenantResolutionMiddleware chama Resolve() com o tenant encontrado.
/// </summary>
public class TenantContext : ITenantContext
{
    private Tenant? _tenant;

    public Guid TenantId => _tenant?.Id ?? Guid.Empty;
    public string TenantSlug => _tenant?.Slug ?? string.Empty;
    public string TenantName => _tenant?.Name ?? string.Empty;
    public string? LogoUrl => _tenant?.LogoUrl;
    public string? CoverImageUrl => _tenant?.CoverImageUrl;
    public string? PrimaryColor => _tenant?.PrimaryColor;
    public string? Tagline => _tenant?.Tagline;
    public string? InstagramUrl => _tenant?.InstagramUrl;
    public string? FacebookUrl => _tenant?.FacebookUrl;
    public string? WhatsappNumber => _tenant?.WhatsappNumber;
    public bool PeakHoursEnabled => _tenant?.PeakHoursEnabled ?? false;
    public int? CancelationWindowHours => _tenant?.CancelationWindowHours;
    public bool IsResolved => _tenant is not null;

    public void Resolve(Tenant tenant)
    {
        _tenant = tenant;
    }
}
