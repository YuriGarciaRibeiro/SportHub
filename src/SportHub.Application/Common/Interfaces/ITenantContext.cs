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
    string? PrimaryColor { get; }

    /// <summary>Nome do schema PostgreSQL. Ex: "tenant_academia_silva"</summary>
    string Schema { get; }

    /// <summary>False antes do middleware resolver. Handlers não devem executar se false.</summary>
    bool IsResolved { get; }

    void Resolve(Tenant tenant);
}
