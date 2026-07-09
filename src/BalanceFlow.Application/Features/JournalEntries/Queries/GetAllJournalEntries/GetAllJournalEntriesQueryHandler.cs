namespace BalanceFlow.Application.Features.JournalEntries.Queries.GetAllJournalEntries;

/// <summary>
/// Handles <see cref="GetAllJournalEntriesQuery"/> — retrieves a paginated list
/// of journal entries (with lines), maps to DTOs, and wraps in a paginated result.
/// </summary>
public sealed class GetAllJournalEntriesQueryHandler
    : IRequestHandler<GetAllJournalEntriesQuery, Result<PagedResult<JournalEntryDto>>>
{
    private readonly IJournalEntryRepository _journalEntryRepository;

    public GetAllJournalEntriesQueryHandler(IJournalEntryRepository journalEntryRepository)
    {
        _journalEntryRepository = journalEntryRepository;
    }

    public async Task<Result<PagedResult<JournalEntryDto>>> Handle(
        GetAllJournalEntriesQuery request,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _journalEntryRepository.GetAllAsync(
            request.PageNumber, request.PageSize, cancellationToken);

        var dtos = items.Select(e => e.ToDto()).ToList();

        var pagedResult = new PagedResult<JournalEntryDto>(
            dtos, totalCount, request.PageNumber, request.PageSize);

        return Result<PagedResult<JournalEntryDto>>.Success(pagedResult);
    }
}
