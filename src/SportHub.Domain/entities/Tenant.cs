using System.Text.Json.Serialization;
using Domain.Enums;

namespace Domain.Entities;

public class Tenant
{
    [JsonInclude] public Guid Id { get; private set; }

    /// <summary>
    /// Identificador único na URL. Ex: "academiasilva" → academiasilva.sporthub.app
    /// Regras: só letras minúsculas, números e hífens. Máx 63 chars (limite DNS).
    /// </summary>
    [JsonInclude] public string Slug { get; private set; } = null!;

    /// <summary>Nome exibido no sistema. Ex: "Academia Silva Esportes"</summary>
    [JsonInclude] public string Name { get; private set; } = null!;

    [JsonInclude] public TenantStatus Status { get; private set; } = TenantStatus.Active;

    // Branding
    [JsonInclude] public string? LogoUrl { get; private set; }
    [JsonInclude] public string? PrimaryColor { get; private set; }

    // Futuro: domínio customizado do cliente
    [JsonInclude] public string? CustomDomain { get; private set; }

    // Dados do dono do tenant
    [JsonInclude] public string? OwnerFirstName { get; private set; }
    [JsonInclude] public string? OwnerLastName { get; private set; }
    [JsonInclude] public string? OwnerEmail { get; private set; }

    [JsonInclude] public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    // Constructor for EF Core and JSON Deserialization (Cache)
    public Tenant() { }

    public static Tenant Create(string slug, string name, string? ownerFirstName = null, string? ownerLastName = null, string? ownerEmail = null)
    {
        return new Tenant
        {
            Id = Guid.NewGuid(),
            Slug = slug.ToLowerInvariant(),
            Name = name,
            OwnerFirstName = ownerFirstName,
            OwnerLastName = ownerLastName,
            OwnerEmail = ownerEmail,
            Status = TenantStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateBranding(string? logoUrl, string? primaryColor)
    {
        LogoUrl = logoUrl;
        PrimaryColor = primaryColor;
    }

    public void UpdateSettings(string name, string? logoUrl, string? primaryColor)
    {
        Name = name;
        LogoUrl = logoUrl;
        PrimaryColor = primaryColor;
    }

    public void Suspend() => Status = TenantStatus.Suspended;
    public void Activate() => Status = TenantStatus.Active;
    public void Cancel() => Status = TenantStatus.Canceled;

    public void UpdateOwnerInfo(string? firstName, string? lastName, string email)
    {
        OwnerFirstName = firstName;
        OwnerLastName = lastName;
        OwnerEmail = email;
    }

    /// <summary>
    /// Deriva o nome do schema PostgreSQL a partir do slug.
    /// Ex: "academia-silva" → "tenant_academia_silva"
    /// </summary>
    public string GetSchemaName() =>
        string.IsNullOrWhiteSpace(Slug) ? "public" : $"tenant_{Slug.Replace("-", "_")}";
}
