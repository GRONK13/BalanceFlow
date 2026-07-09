namespace BalanceFlow.Domain.Exceptions;

/// <summary>
/// Thrown when a journal entry violates the fundamental double-entry bookkeeping rule:
/// the sum of all debit amounts must equal the sum of all credit amounts.
/// </summary>
public sealed class UnbalancedJournalEntryException : DomainException
{
    public decimal TotalDebits { get; }
    public decimal TotalCredits { get; }
    public decimal Difference => Math.Abs(TotalDebits - TotalCredits);

    public UnbalancedJournalEntryException(decimal totalDebits, decimal totalCredits)
        : base($"Journal entry is unbalanced. " +
               $"Total Debits: {totalDebits:C}, Total Credits: {totalCredits:C}, " +
               $"Difference: {Math.Abs(totalDebits - totalCredits):C}.")
    {
        TotalDebits = totalDebits;
        TotalCredits = totalCredits;
    }
}
