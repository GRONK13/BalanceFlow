namespace BalanceFlow.Application.Features.JournalEntries.Queries.GetAllJournalEntries;

/// <summary>
/// Query to retrieve a paginated list of journal entries,
/// ordered by transaction date descending (most recent first).
/// </summary>
public sealed record GetAllJournalEntriesQuery(
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<Result<PagedResult<JournalEntryDto>>>;
