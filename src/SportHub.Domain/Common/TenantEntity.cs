namespace SportHub.Domain.Common;

public abstract class TenantEntity : AuditEntity
{
    public Guid TenantId { get; set; }
}
