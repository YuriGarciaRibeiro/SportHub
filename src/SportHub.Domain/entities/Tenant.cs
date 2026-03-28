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
    [JsonInclude] public string? CoverImageUrl { get; private set; }
    [JsonInclude] public string? PrimaryColor { get; private set; }
    [JsonInclude] public string? Tagline { get; private set; }

    // Redes sociais globais da marca
    [JsonInclude] public string? InstagramUrl { get; private set; }
    [JsonInclude] public string? FacebookUrl { get; private set; }
    [JsonInclude] public string? WhatsappNumber { get; private set; }

    // Futuro: domínio customizado do cliente
    [JsonInclude] public string? CustomDomain { get; private set; }

    // Dados do dono do tenant
    [JsonInclude] public string? OwnerFirstName { get; private set; }
    [JsonInclude] public string? OwnerLastName { get; private set; }
    [JsonInclude] public string? OwnerEmail { get; private set; }

    [JsonInclude] public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    [JsonInclude] public bool PeakHoursEnabled { get; private set; } = false;

    [JsonInclude] public int CancelationWindowHours { get; private set; } = 0;

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

    public void UpdateCoverImage(string? coverImageUrl)
    {
        CoverImageUrl = coverImageUrl;
    }

    public void UpdateSettings(string name, string? logoUrl, string? primaryColor, string? tagline, int? cancelationWindowHours, bool peakHoursEnabled)
    {
        Name = name;
        LogoUrl = logoUrl;
        PrimaryColor = primaryColor;
        Tagline = tagline;
        CancelationWindowHours = cancelationWindowHours ?? CancelationWindowHours;
        PeakHoursEnabled = peakHoursEnabled;
    }

    public void UpdateSocialMedia(string? instagramUrl, string? facebookUrl, string? whatsappNumber)
    {
        InstagramUrl = instagramUrl;
        FacebookUrl = facebookUrl;
        WhatsappNumber = whatsappNumber;
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

}
