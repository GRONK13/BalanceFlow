namespace BalanceFlow.Application.Features.JournalEntries.Commands.PostJournalEntry;

/// <summary>
/// Command to finalize (post) a draft journal entry.
/// This triggers the domain's double-entry balance validation:
/// total debits must equal total credits.
/// </summary>
public sealed record PostJournalEntryCommand(Guid Id) : IRequest<Result<JournalEntryDto>>;
