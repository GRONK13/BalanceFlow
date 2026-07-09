namespace BalanceFlow.Application.DTOs;

/// <summary>
/// Data transfer object for <see cref="JournalEntry"/> aggregate roots.
/// Includes nested <see cref="JournalEntryLineDto"/> records and
/// pre-computed totals so the client doesn't need to recalculate them.
/// </summary>
public sealed record JournalEntryDto(
    Guid Id,
    string ReferenceNumber,
    DateTime TransactionDate,
    string? Description,
    bool IsPosted,
    DateTime? PostedAt,
    decimal TotalDebits,
    decimal TotalCredits,
    bool IsBalanced,
    IReadOnlyList<JournalEntryLineDto> Lines,
    DateTime CreatedAt,
    DateTime? ModifiedAt
);
