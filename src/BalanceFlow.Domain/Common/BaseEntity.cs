namespace BalanceFlow.Domain.Common;

/// <summary>
/// Abstract base class for all domain entities.
/// Provides identity, audit timestamps, and soft-delete capability.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Unique identifier for the entity. Uses <see cref="Guid"/> to support
    /// distributed ID generation without database coordination.
    /// </summary>
    public Guid Id { get; private set; } = Guid.NewGuid();

    /// <summary>UTC timestamp indicating when the entity was first persisted.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>UTC timestamp of the most recent modification. Null if never modified after creation.</summary>
    public DateTime? ModifiedAt { get; set; }

    /// <summary>
    /// Soft-delete flag. When <c>true</c>, the entity is logically deleted
    /// and should be excluded from standard queries via a global query filter.
    /// </summary>
    public bool IsDeleted { get; private set; }

    /// <summary>
    /// Marks the entity as logically deleted and records the deletion timestamp.
    /// </summary>
    public void SoftDelete()
    {
        IsDeleted = true;
        ModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Restores a soft-deleted entity and records the restoration timestamp.
    /// </summary>
    public void Restore()
    {
        IsDeleted = false;
        ModifiedAt = DateTime.UtcNow;
    }
}
