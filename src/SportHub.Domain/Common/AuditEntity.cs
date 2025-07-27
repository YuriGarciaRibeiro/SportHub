using System;

namespace Domain.Common;

public abstract class AuditEntity
{
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;
    public Guid CreatedBy { get; private set; }
    public Guid UpdatedBy { get; private set; }

    public bool IsDeleted { get; private set; } = false;
    public Guid? DeletedBy { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public void SetCreated(Guid userId)
    {
        CreatedBy = userId;
        UpdatedBy = userId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public void SetUpdated(Guid userId)
    {
        UpdatedBy = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsDeleted(Guid userId)
    {
        IsDeleted = true;
        DeletedBy = userId;
        DeletedAt = DateTime.UtcNow;
    }
    
    public void Restore()
    {
        IsDeleted = false;
        DeletedBy = null;
        DeletedAt = null;
    }
}
