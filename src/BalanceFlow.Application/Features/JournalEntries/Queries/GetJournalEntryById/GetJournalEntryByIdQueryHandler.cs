namespace BalanceFlow.Application.Features.JournalEntries.Queries.GetJournalEntryById;

/// <summary>
/// Handles <see cref="GetJournalEntryByIdQuery"/> — loads a journal entry
/// (with eager-loaded lines and account navigation properties) and maps to DTO.
/// </summary>
public sealed class GetJournalEntryByIdQueryHandler
    : IRequestHandler<GetJournalEntryByIdQuery, Result<JournalEntryDto>>
{
    private readonly IJournalEntryRepository _journalEntryRepository;

    public GetJournalEntryByIdQueryHandler(IJournalEntryRepository journalEntryRepository)
    {
        _journalEntryRepository = journalEntryRepository;
    }

    public async Task<Result<JournalEntryDto>> Handle(
        GetJournalEntryByIdQuery request,
        CancellationToken cancellationToken)
    {
        var journalEntry = await _journalEntryRepository.GetByIdAsync(
            request.Id, cancellationToken);

        if (journalEntry is null)
            return Result<JournalEntryDto>.Failure(
                $"Journal entry with ID '{request.Id}' was not found.");

        return Result<JournalEntryDto>.Success(journalEntry.ToDto());
    }
}
