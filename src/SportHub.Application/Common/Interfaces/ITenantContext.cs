using Domain.Entities;

namespace Application.Common.Interfaces;

/// <summary>
/// Contexto do tenant resolvido para a request atual.
/// Registrado como Scoped — uma instância por request HTTP.
/// </summary>
public interface ITenantContext
{
    Guid TenantId { get; }
    string TenantSlug { get; }
    string TenantName { get; }
    string? LogoUrl { get; }
    string? CoverImageUrl { get; }
    string? PrimaryColor { get; }
    string? Tagline { get; }
    string? InstagramUrl { get; }
    string? FacebookUrl { get; }
    string? WhatsappNumber { get; }
    bool PeakHoursEnabled { get; }

    bool IsResolved { get; }

    void Resolve(Tenant tenant);
}
