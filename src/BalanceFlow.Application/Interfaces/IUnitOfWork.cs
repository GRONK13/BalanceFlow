namespace BalanceFlow.Application.Interfaces;

/// <summary>
/// Abstracts the "save all pending changes" operation.
/// In Entity Framework Core, this maps to <c>DbContext.SaveChangesAsync()</c>.
///
/// <para><strong>Why a separate interface?</strong>
/// Command handlers often modify multiple entities (e.g., create a JournalEntry
/// and its Lines). The Unit of Work ensures all changes commit atomically —
/// either everything succeeds or nothing does. Without it, you'd have to call
/// "save" inside each repository, risking partial writes.</para>
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Persists all pending changes to the database in a single transaction.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The number of entities written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
