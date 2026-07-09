namespace BalanceFlow.Application.Features.JournalEntries.Commands.PostJournalEntry;

/// <summary>
/// Handles <see cref="PostJournalEntryCommand"/>:
/// 1. Loads the draft journal entry.
/// 2. Calls <see cref="JournalEntry.Post()"/> which validates the double-entry
///    bookkeeping rule (debits == credits) and marks the entry as immutable.
/// 3. Catches domain exceptions and converts them to <see cref="Result"/> failures.
/// 4. Persists the posted state.
///
/// <para><strong>Architecture Note — Domain vs. Application validation:</strong>
/// The handler doesn't calculate debits/credits itself. It delegates to the
/// domain's <c>Post()</c> method, which owns that business rule. The handler's
/// job is just orchestration: load → call domain method → persist → return result.
/// This is the Clean Architecture "Application layer coordinates, Domain layer validates" pattern.</para>
/// </summary>
public sealed class PostJournalEntryCommandHandler
    : IRequestHandler<PostJournalEntryCommand, Result<JournalEntryDto>>
{
    private readonly IJournalEntryRepository _journalEntryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PostJournalEntryCommandHandler(
        IJournalEntryRepository journalEntryRepository,
        IUnitOfWork unitOfWork)
    {
        _journalEntryRepository = journalEntryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<JournalEntryDto>> Handle(
        PostJournalEntryCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Load the entry (with lines) from the repository.
        var journalEntry = await _journalEntryRepository.GetByIdAsync(
            request.Id, cancellationToken);

        if (journalEntry is null)
            return Result<JournalEntryDto>.Failure(
                $"Journal entry with ID '{request.Id}' was not found.");

        // 2. Attempt to post — the domain validates the double-entry rule.
        try
        {
            journalEntry.Post();
        }
        catch (UnbalancedJournalEntryException ex)
        {
            // Domain threw because debits ≠ credits. Convert to a Result failure.
            return Result<JournalEntryDto>.Failure($"Cannot post: {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            // Domain threw because entry is already posted or has < 2 lines.
            return Result<JournalEntryDto>.Failure(ex.Message);
        }

        // 3. Persist the posted state.
        _journalEntryRepository.Update(journalEntry);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 4. Return the finalized entry.
        return Result<JournalEntryDto>.Success(journalEntry.ToDto());
    }
}
