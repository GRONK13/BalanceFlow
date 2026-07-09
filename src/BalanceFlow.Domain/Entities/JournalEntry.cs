namespace BalanceFlow.Domain.Entities;

/// <summary>
/// Represents a journal entry — the atomic unit of record in double-entry bookkeeping.
/// A journal entry groups two or more <see cref="JournalEntryLine"/> records whose
/// total debits must equal total credits before the entry can be finalized (posted).
///
/// <para><strong>Lifecycle:</strong> Draft → Posted (immutable once posted).</para>
/// </summary>
public sealed class JournalEntry : BaseEntity
{
    /// <summary>
    /// A sequential, human-readable reference number (e.g., "JE-000042").
    /// Typically assigned by the Application layer or a database sequence.
    /// </summary>
    public string ReferenceNumber { get; private set; } = string.Empty;

    /// <summary>The business date of the transaction, independent of the audit timestamps.</summary>
    public DateTime TransactionDate { get; private set; }

    /// <summary>High-level description or memo for the journal entry.</summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Indicates whether the entry has been finalized.
    /// Once posted, the entry and its lines become immutable.
    /// </summary>
    public bool IsPosted { get; private set; }

    /// <summary>UTC timestamp of when the entry was posted. Null while in draft state.</summary>
    public DateTime? PostedAt { get; private set; }

    // Encapsulated collection — external code must use AddLine().
    private readonly List<JournalEntryLine> _lines = [];

    /// <summary>
    /// The collection of debit/credit lines that compose this journal entry.
    /// Exposed as read-only to enforce mutation through domain methods only.
    /// </summary>
    public IReadOnlyCollection<JournalEntryLine> Lines => _lines.AsReadOnly();

    // EF Core requires a parameterless constructor.
    private JournalEntry() { }

    /// <summary>
    /// Creates a new draft journal entry.
    /// </summary>
    /// <param name="referenceNumber">Unique reference number (required).</param>
    /// <param name="transactionDate">The business date of the transaction.</param>
    /// <param name="description">Optional memo or narrative.</param>
    public JournalEntry(string referenceNumber, DateTime transactionDate, string? description = null)
    {
        SetReferenceNumber(referenceNumber);
        TransactionDate = transactionDate;
        Description = description?.Trim();
    }

    /// <summary>Updates the reference number with validation.</summary>
    public void SetReferenceNumber(string referenceNumber)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(referenceNumber, nameof(referenceNumber));

        if (referenceNumber.Length > 50)
            throw new ArgumentException("Reference number must not exceed 50 characters.", nameof(referenceNumber));

        EnsureDraft();
        ReferenceNumber = referenceNumber.Trim();
    }

    /// <summary>Updates the transaction date.</summary>
    public void SetTransactionDate(DateTime transactionDate)
    {
        EnsureDraft();
        TransactionDate = transactionDate;
    }

    /// <summary>Updates the description / memo.</summary>
    public void SetDescription(string? description)
    {
        EnsureDraft();
        Description = description?.Trim();
    }

    /// <summary>
    /// Adds a new posting line to this journal entry.
    /// </summary>
    /// <param name="line">A fully constructed <see cref="JournalEntryLine"/>.</param>
    /// <exception cref="InvalidOperationException">Thrown when the entry has already been posted.</exception>
    public void AddLine(JournalEntryLine line)
    {
        ArgumentNullException.ThrowIfNull(line, nameof(line));
        EnsureDraft();

        _lines.Add(line);
    }

    /// <summary>
    /// Removes a posting line from this journal entry.
    /// </summary>
    /// <param name="lineId">The ID of the line to remove.</param>
    /// <exception cref="InvalidOperationException">Thrown when the entry has already been posted.</exception>
    /// <exception cref="ArgumentException">Thrown when no line with the specified ID exists.</exception>
    public void RemoveLine(Guid lineId)
    {
        EnsureDraft();

        var line = _lines.FirstOrDefault(l => l.Id == lineId)
            ?? throw new ArgumentException($"Journal entry line with ID '{lineId}' was not found.", nameof(lineId));

        _lines.Remove(line);
    }

    /// <summary>
    /// Calculates the sum of all debit amounts across lines.
    /// </summary>
    public decimal TotalDebits => _lines.Sum(l => l.DebitAmount);

    /// <summary>
    /// Calculates the sum of all credit amounts across lines.
    /// </summary>
    public decimal TotalCredits => _lines.Sum(l => l.CreditAmount);

    /// <summary>
    /// Determines whether the journal entry is balanced (total debits == total credits).
    /// </summary>
    public bool IsBalanced => TotalDebits == TotalCredits && _lines.Count > 0;

    /// <summary>
    /// Finalizes (posts) the journal entry after validating all double-entry bookkeeping rules.
    ///
    /// <para><strong>Rules enforced:</strong></para>
    /// <list type="bullet">
    ///   <item>The entry must contain at least two lines.</item>
    ///   <item>Total debits must exactly equal total credits.</item>
    /// </list>
    ///
    /// Once posted, the entry and all its lines become immutable.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the entry is already posted or has fewer than two lines.
    /// </exception>
    /// <exception cref="UnbalancedJournalEntryException">
    /// Thrown when total debits do not equal total credits.
    /// </exception>
    public void Post()
    {
        EnsureDraft();

        if (_lines.Count < 2)
            throw new InvalidOperationException(
                $"A journal entry must have at least 2 lines to be posted. Current line count: {_lines.Count}.");

        var totalDebits = TotalDebits;
        var totalCredits = TotalCredits;

        if (totalDebits != totalCredits)
            throw new UnbalancedJournalEntryException(totalDebits, totalCredits);

        IsPosted = true;
        PostedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Guards all mutation methods — once an entry is posted it is immutable.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the entry has already been posted.</exception>
    private void EnsureDraft()
    {
        if (IsPosted)
            throw new InvalidOperationException(
                $"Journal entry '{ReferenceNumber}' has already been posted and cannot be modified.");
    }
}
