namespace BalanceFlow.Application.Features.JournalEntries.Commands.CreateJournalEntry;

/// <summary>
/// Handles <see cref="CreateJournalEntryCommand"/>:
/// 1. Validates reference number uniqueness.
/// 2. Validates that all referenced accounts exist and are active.
/// 3. Creates the <see cref="JournalEntry"/> aggregate root with its lines.
/// 4. Persists as a draft (not yet posted).
///
/// <para><strong>Design Decision — Why validate accounts here?</strong>
/// The domain entity's <c>JournalEntryLine</c> constructor only takes an
/// <c>AccountId</c> (a <see cref="Guid"/>). It can't verify the account
/// exists because the Domain layer has no database access. That cross-entity
/// validation is the Application layer's responsibility.</para>
/// </summary>
public sealed class CreateJournalEntryCommandHandler
    : IRequestHandler<CreateJournalEntryCommand, Result<JournalEntryDto>>
{
    private readonly IJournalEntryRepository _journalEntryRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateJournalEntryCommandHandler(
        IJournalEntryRepository journalEntryRepository,
        IAccountRepository accountRepository,
        IUnitOfWork unitOfWork)
    {
        _journalEntryRepository = journalEntryRepository;
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<JournalEntryDto>> Handle(
        CreateJournalEntryCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Business rule: reference numbers must be unique.
        var existing = await _journalEntryRepository.GetByReferenceNumberAsync(
            request.ReferenceNumber, cancellationToken);

        if (existing is not null)
            return Result<JournalEntryDto>.Failure(
                $"A journal entry with reference number '{request.ReferenceNumber}' already exists.");

        // 2. Validate all referenced accounts exist and are active.
        foreach (var lineDto in request.Lines)
        {
            var account = await _accountRepository.GetByIdAsync(
                lineDto.AccountId, cancellationToken);

            if (account is null)
                return Result<JournalEntryDto>.Failure(
                    $"Account with ID '{lineDto.AccountId}' was not found.");

            if (!account.IsActive)
                return Result<JournalEntryDto>.Failure(
                    $"Account '{account.AccountCode}' is inactive and cannot be posted to.");
        }

        // 3. Create the aggregate root and add lines.
        var journalEntry = new JournalEntry(
            request.ReferenceNumber,
            request.TransactionDate,
            request.Description);

        foreach (var lineDto in request.Lines)
        {
            var line = new JournalEntryLine(
                lineDto.AccountId,
                lineDto.DebitAmount,
                lineDto.CreditAmount,
                lineDto.Description);

            journalEntry.AddLine(line);
        }

        // 4. Persist the draft entry.
        await _journalEntryRepository.AddAsync(journalEntry, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<JournalEntryDto>.Success(journalEntry.ToDto());
    }
}
