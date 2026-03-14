using System;

namespace SportHub.Domain.Common;

public abstract class AuditEntity
{
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public Guid CreatedBy { get; private set; }
    public Guid UpdatedBy { get; private set; }

    public bool IsDeleted { get; private set; } = false;
    public Guid? DeletedBy { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public void SetCreated(Guid userId, DateTime? utcNow = null)
    {
        var now = utcNow ?? DateTime.UtcNow;
        CreatedBy = userId;
        UpdatedBy = userId;
        CreatedAt = now;
        UpdatedAt = now;
    }

    public void SetUpdated(Guid userId, DateTime? utcNow = null)
    {
        var now = utcNow ?? DateTime.UtcNow;
        UpdatedBy = userId;
        UpdatedAt = now;
    }

    public void MarkAsDeleted(Guid userId, DateTime? utcNow = null)
    {
        var now = utcNow ?? DateTime.UtcNow;
        IsDeleted = true;
        DeletedBy = userId;
        DeletedAt = now;
    }

    public void Restore()
    {
        IsDeleted = false;
        DeletedBy = null;
        DeletedAt = null;
    }
}
