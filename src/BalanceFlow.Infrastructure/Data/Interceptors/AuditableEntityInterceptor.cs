using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BalanceFlow.Infrastructure.Data.Interceptors;

/// <summary>
/// EF Core SaveChanges interceptor that automatically sets audit metadata fields
/// (CreatedAt, ModifiedAt) before entities are written to the database.
///
/// <para><strong>C# Concept — SaveChangesInterceptor:</strong>
/// This runs automatically right before SaveChangesAsync is called. It scans EF Core's
/// change tracker for entities deriving from <see cref="BaseEntity"/> and stamps them with the current UTC time.</para>
/// </summary>
public sealed class AuditableEntityInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        UpdateAuditProperties(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateAuditProperties(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void UpdateAuditProperties(DbContext? context)
    {
        if (context is null) return;

        var entries = context.ChangeTracker.Entries<BaseEntity>();

        var utcNow = DateTime.UtcNow;

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = utcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.ModifiedAt = utcNow;
            }
        }
    }
}
