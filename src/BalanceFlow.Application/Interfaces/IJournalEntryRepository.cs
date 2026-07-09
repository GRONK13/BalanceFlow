namespace BalanceFlow.Application.Interfaces;

/// <summary>
/// Data access contract for <see cref="JournalEntry"/> aggregate roots.
/// Implementations must eager-load <see cref="JournalEntry.Lines"/> and
/// each line's <see cref="JournalEntryLine.Account"/> navigation property
/// so that mapping to DTOs can include account codes and names.
/// </summary>
public interface IJournalEntryRepository
{
    /// <summary>
    /// Retrieves a journal entry by ID, including all its lines and their associated accounts.
    /// Returns <c>null</c> if not found or soft-deleted.
    /// </summary>
    Task<JournalEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paginated list of non-deleted journal entries (with lines),
    /// ordered by transaction date descending (most recent first).
    /// </summary>
    Task<(IReadOnlyList<JournalEntry> Items, int TotalCount)> GetAllAsync(
        int pageNumber, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a journal entry by its unique reference number.
    /// Used for uniqueness validation when creating entries.
    /// </summary>
    Task<JournalEntry?> GetByReferenceNumberAsync(string referenceNumber, CancellationToken cancellationToken = default);

    /// <summary>Adds a new journal entry to the data store.</summary>
    Task AddAsync(JournalEntry journalEntry, CancellationToken cancellationToken = default);

    /// <summary>Marks an existing journal entry as modified.</summary>
    void Update(JournalEntry journalEntry);
}
