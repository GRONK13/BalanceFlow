namespace BalanceFlow.Domain.Entities;

/// <summary>
/// Represents a single line (posting) within a <see cref="JournalEntry"/>.
/// Each line debits or credits exactly one <see cref="Account"/>.
///
/// Invariant: exactly one of <see cref="DebitAmount"/> or <see cref="CreditAmount"/>
/// must be greater than zero; the other must be zero.
/// </summary>
public sealed class JournalEntryLine : BaseEntity
{
    /// <summary>Foreign key to the parent journal entry.</summary>
    public Guid JournalEntryId { get; private set; }

    /// <summary>Navigation property to the parent journal entry.</summary>
    public JournalEntry JournalEntry { get; private set; } = null!;

    /// <summary>Foreign key to the account being posted to.</summary>
    public Guid AccountId { get; private set; }

    /// <summary>Navigation property to the target account.</summary>
    public Account Account { get; private set; } = null!;

    /// <summary>
    /// The debit amount for this line. Zero if this line is a credit posting.
    /// Uses <c>decimal</c> for exact monetary arithmetic.
    /// </summary>
    public decimal DebitAmount { get; private set; }

    /// <summary>
    /// The credit amount for this line. Zero if this line is a debit posting.
    /// Uses <c>decimal</c> for exact monetary arithmetic.
    /// </summary>
    public decimal CreditAmount { get; private set; }

    /// <summary>Optional memo describing the purpose of this specific line.</summary>
    public string? Description { get; private set; }

    // EF Core requires a parameterless constructor.
    private JournalEntryLine() { }

    /// <summary>
    /// Creates a new journal entry line with full validation.
    /// </summary>
    /// <param name="accountId">The account to post against.</param>
    /// <param name="debitAmount">Debit amount (zero if crediting).</param>
    /// <param name="creditAmount">Credit amount (zero if debiting).</param>
    /// <param name="description">Optional line-level memo.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when both amounts are zero, both are non-zero,
    /// or either amount is negative.
    /// </exception>
    public JournalEntryLine(Guid accountId, decimal debitAmount, decimal creditAmount, string? description = null)
    {
        if (accountId == Guid.Empty)
            throw new ArgumentException("Account ID must not be empty.", nameof(accountId));

        ValidateAmounts(debitAmount, creditAmount);

        AccountId = accountId;
        DebitAmount = debitAmount;
        CreditAmount = creditAmount;
        Description = description?.Trim();
    }

    /// <summary>
    /// Enforces the single-sided posting rule: a line is either a debit or a credit, never both and never neither.
    /// </summary>
    private static void ValidateAmounts(decimal debitAmount, decimal creditAmount)
    {
        if (debitAmount < 0)
            throw new ArgumentException("Debit amount must not be negative.", nameof(debitAmount));

        if (creditAmount < 0)
            throw new ArgumentException("Credit amount must not be negative.", nameof(creditAmount));

        if (debitAmount == 0 && creditAmount == 0)
            throw new ArgumentException("A journal entry line must have either a debit or a credit amount greater than zero.");

        if (debitAmount > 0 && creditAmount > 0)
            throw new ArgumentException("A journal entry line cannot have both a debit and a credit amount. Post separate lines instead.");
    }
}
