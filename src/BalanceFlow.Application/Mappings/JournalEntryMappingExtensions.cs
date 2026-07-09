namespace BalanceFlow.Application.Mappings;

/// <summary>
/// Extension methods that convert <see cref="JournalEntry"/> and
/// <see cref="JournalEntryLine"/> domain entities to their DTO counterparts.
/// </summary>
public static class JournalEntryMappingExtensions
{
    /// <summary>
    /// Converts a <see cref="JournalEntry"/> (including all child lines) to a <see cref="JournalEntryDto"/>.
    /// </summary>
    public static JournalEntryDto ToDto(this JournalEntry entry) => new(
        Id: entry.Id,
        ReferenceNumber: entry.ReferenceNumber,
        TransactionDate: entry.TransactionDate,
        Description: entry.Description,
        IsPosted: entry.IsPosted,
        PostedAt: entry.PostedAt,
        TotalDebits: entry.TotalDebits,
        TotalCredits: entry.TotalCredits,
        IsBalanced: entry.IsBalanced,
        Lines: entry.Lines.Select(l => l.ToDto()).ToList(),
        CreatedAt: entry.CreatedAt,
        ModifiedAt: entry.ModifiedAt
    );

    /// <summary>
    /// Converts a <see cref="JournalEntryLine"/> to a <see cref="JournalEntryLineDto"/>.
    /// Includes denormalized account code/name via the navigation property
    /// (null-safe — if the Account isn't loaded, these will be <c>null</c>).
    /// </summary>
    public static JournalEntryLineDto ToDto(this JournalEntryLine line) => new(
        Id: line.Id,
        AccountId: line.AccountId,
        AccountCode: line.Account?.AccountCode,
        AccountName: line.Account?.Name,
        DebitAmount: line.DebitAmount,
        CreditAmount: line.CreditAmount,
        Description: line.Description
    );
}
