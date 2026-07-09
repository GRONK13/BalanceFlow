namespace BalanceFlow.Application.Features.JournalEntries.Queries.GetJournalEntryById;

/// <summary>
/// Query to retrieve a single journal entry (with all lines and account details) by ID.
/// </summary>
public sealed record GetJournalEntryByIdQuery(Guid Id) : IRequest<Result<JournalEntryDto>>;
