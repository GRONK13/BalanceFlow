namespace BalanceFlow.Application.Features.JournalEntries.Commands.CreateJournalEntry;

/// <summary>
/// Command to create a new draft journal entry with its posting lines.
/// The entry starts in draft state and must be explicitly posted via
/// <see cref="PostJournalEntry.PostJournalEntryCommand"/> to finalize it.
/// </summary>
public sealed record CreateJournalEntryCommand(
    string ReferenceNumber,
    DateTime TransactionDate,
    string? Description,
    List<CreateJournalEntryLineDto> Lines
) : IRequest<Result<JournalEntryDto>>;

/// <summary>
/// Nested DTO for the lines within a <see cref="CreateJournalEntryCommand"/>.
/// Each line must have either a <see cref="DebitAmount"/> OR a <see cref="CreditAmount"/>
/// greater than zero (never both, never neither).
/// </summary>
public sealed record CreateJournalEntryLineDto(
    Guid AccountId,
    decimal DebitAmount,
    decimal CreditAmount,
    string? Description
);
