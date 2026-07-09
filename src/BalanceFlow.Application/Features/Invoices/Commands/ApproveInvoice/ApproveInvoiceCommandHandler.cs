namespace BalanceFlow.Application.Features.Invoices.Commands.ApproveInvoice;

/// <summary>
/// Handles <see cref="ApproveInvoiceCommand"/>:
/// 1. Verifies target AP Account exists, is active, and is a Liability account.
/// 2. Approves the invoice, yielding a balanced double-entry Ledger JournalEntry.
/// 3. Registers the new JournalEntry.
/// 4. Saves everything in an atomic database transaction.
/// </summary>
public sealed class ApproveInvoiceCommandHandler
    : IRequestHandler<ApproveInvoiceCommand, Result<InvoiceDto>>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IJournalEntryRepository _journalEntryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ApproveInvoiceCommandHandler(
        IInvoiceRepository invoiceRepository,
        IAccountRepository accountRepository,
        IJournalEntryRepository journalEntryRepository,
        IUnitOfWork unitOfWork)
    {
        _invoiceRepository = invoiceRepository;
        _accountRepository = accountRepository;
        _journalEntryRepository = journalEntryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<InvoiceDto>> Handle(
        ApproveInvoiceCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Fetch the invoice
        var invoice = await _invoiceRepository.GetByIdAsync(request.Id, cancellationToken);
        if (invoice is null)
        {
            return Result<InvoiceDto>.Failure($"Invoice with ID '{request.Id}' was not found.");
        }

        // 2. Fetch and validate target Accounts Payable (AP) liability account
        var apAccount = await _accountRepository.GetByIdAsync(request.AccountsPayableAccountId, cancellationToken);
        if (apAccount is null)
        {
            return Result<InvoiceDto>.Failure($"Accounts Payable account with ID '{request.AccountsPayableAccountId}' was not found.");
        }

        if (!apAccount.IsActive)
        {
            return Result<InvoiceDto>.Failure($"Accounts Payable account '{apAccount.AccountCode}' is currently inactive.");
        }

        if (apAccount.Type != AccountType.Liability)
        {
            return Result<InvoiceDto>.Failure(
                $"Accounts Payable account '{apAccount.AccountCode}' must be of classification Liability (received: {apAccount.Type}).");
        }

        // 3. Approve invoice and draft the double-entry JournalEntry
        JournalEntry ledgerEntry;
        try
        {
            ledgerEntry = invoice.Approve(request.AccountsPayableAccountId);
        }
        catch (InvalidOperationException ex)
        {
            return Result<InvoiceDto>.Failure(ex.Message);
        }

        // 4. Save updates and append the ledger posting
        _invoiceRepository.Update(invoice);
        await _journalEntryRepository.AddAsync(ledgerEntry, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<InvoiceDto>.Success(invoice.ToDto());
    }
}
