namespace BalanceFlow.Application.Features.JournalEntries.Commands.DeleteJournalEntry;

/// <summary>
/// Handles <see cref="DeleteJournalEntryCommand"/>:
/// - Validates the entry exists.
/// - Blocks deletion of posted entries (accounting immutability rule).
/// - Soft-deletes draft entries.
/// </summary>
public sealed class DeleteJournalEntryCommandHandler
    : IRequestHandler<DeleteJournalEntryCommand, Result>
{
    private readonly IJournalEntryRepository _journalEntryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteJournalEntryCommandHandler(
        IJournalEntryRepository journalEntryRepository,
        IUnitOfWork unitOfWork)
    {
        _journalEntryRepository = journalEntryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        DeleteJournalEntryCommand request,
        CancellationToken cancellationToken)
    {
        var journalEntry = await _journalEntryRepository.GetByIdAsync(
            request.Id, cancellationToken);

        if (journalEntry is null)
            return Result.Failure(
                $"Journal entry with ID '{request.Id}' was not found.");

        // Accounting rule: posted entries are immutable — never delete them.
        if (journalEntry.IsPosted)
            return Result.Failure(
                $"Journal entry '{journalEntry.ReferenceNumber}' has been posted and cannot be deleted. " +
                "Create a reversing entry instead.");

        journalEntry.SoftDelete();
        _journalEntryRepository.Update(journalEntry);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
