namespace BalanceFlow.Application.Features.JournalEntries.Commands.DeleteJournalEntry;

/// <summary>
/// Command to soft-delete a draft journal entry.
/// Posted entries cannot be deleted — this mirrors real accounting practice
/// where you create a reversing entry instead of deleting.
/// </summary>
public sealed record DeleteJournalEntryCommand(Guid Id) : IRequest<Result>;
