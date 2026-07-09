namespace BalanceFlow.Domain.Entities;

/// <summary>
/// Represents a ledger account in the chart of accounts.
/// Each account belongs to exactly one <see cref="AccountType"/> and
/// serves as a posting target for <see cref="JournalEntryLine"/> records.
/// </summary>
public sealed class Account : BaseEntity
{
    /// <summary>
    /// Human-readable account code (e.g., "1000", "2100").
    /// Must be unique within the chart of accounts.
    /// </summary>
    public string AccountCode { get; private set; } = string.Empty;

    /// <summary>Descriptive name of the account (e.g., "Cash", "Accounts Payable").</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Optional long-form description of the account's purpose.</summary>
    public string? Description { get; private set; }

    /// <summary>The fundamental classification of this account.</summary>
    public AccountType Type { get; private set; }

    /// <summary>
    /// Indicates whether this account is currently active and available for posting.
    /// Inactive accounts are preserved for historical reporting but reject new postings.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    // Navigation — one account can appear on many journal entry lines.
    private readonly List<JournalEntryLine> _journalEntryLines = [];

    /// <summary>All journal entry lines posted against this account.</summary>
    public IReadOnlyCollection<JournalEntryLine> JournalEntryLines => _journalEntryLines.AsReadOnly();

    // EF Core requires a parameterless constructor.
    private Account() { }

    /// <summary>
    /// Creates a new <see cref="Account"/> with full validation.
    /// </summary>
    /// <param name="accountCode">Unique account code (required, max 20 characters).</param>
    /// <param name="name">Account display name (required, max 150 characters).</param>
    /// <param name="type">Account classification.</param>
    /// <param name="description">Optional description.</param>
    /// <exception cref="ArgumentException">Thrown when required fields are invalid.</exception>
    public Account(string accountCode, string name, AccountType type, string? description = null)
    {
        SetAccountCode(accountCode);
        SetName(name);
        Type = type;
        Description = description?.Trim();
    }

    /// <summary>Updates the account code with validation.</summary>
    public void SetAccountCode(string accountCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(accountCode, nameof(accountCode));

        if (accountCode.Length > 20)
            throw new ArgumentException("Account code must not exceed 20 characters.", nameof(accountCode));

        AccountCode = accountCode.Trim();
    }

    /// <summary>Updates the account display name with validation.</summary>
    public void SetName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));

        if (name.Length > 150)
            throw new ArgumentException("Account name must not exceed 150 characters.", nameof(name));

        Name = name.Trim();
    }

    /// <summary>Updates the optional description.</summary>
    public void SetDescription(string? description)
    {
        Description = description?.Trim();
    }

    /// <summary>Deactivates the account, preventing future postings.</summary>
    public void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>Reactivates a previously deactivated account.</summary>
    public void Activate()
    {
        IsActive = true;
    }
}
