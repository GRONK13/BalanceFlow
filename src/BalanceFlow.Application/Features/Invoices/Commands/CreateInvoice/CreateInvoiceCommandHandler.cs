namespace BalanceFlow.Application.Features.Invoices.Commands.CreateInvoice;

/// <summary>
/// Handles <see cref="CreateInvoiceCommand"/>:
/// 1. Verifies that the invoice number is unique for this vendor.
/// 2. Verifies all line accounts exist and are active.
/// 3. Builds the Invoice aggregate root and line items.
/// 4. Saves and returns the drafted invoice DTO.
/// </summary>
public sealed class CreateInvoiceCommandHandler
    : IRequestHandler<CreateInvoiceCommand, Result<InvoiceDto>>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateInvoiceCommandHandler(
        IInvoiceRepository invoiceRepository,
        IAccountRepository accountRepository,
        IUnitOfWork unitOfWork)
    {
        _invoiceRepository = invoiceRepository;
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<InvoiceDto>> Handle(
        CreateInvoiceCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Uniqueness check: Invoice Number + Vendor combination.
        var existing = await _invoiceRepository.GetByInvoiceNumberAndVendorAsync(
            request.InvoiceNumber, request.VendorName, cancellationToken);

        if (existing is not null)
        {
            return Result<InvoiceDto>.Failure(
                $"Invoice number '{request.InvoiceNumber}' has already been recorded for vendor '{request.VendorName}'.");
        }

        // 2. Validate all ledger accounts exist and are active.
        foreach (var itemDto in request.LineItems)
        {
            var account = await _accountRepository.GetByIdAsync(itemDto.AccountId, cancellationToken);
            if (account is null)
            {
                return Result<InvoiceDto>.Failure($"Account ID '{itemDto.AccountId}' was not found.");
            }

            if (!account.IsActive)
            {
                return Result<InvoiceDto>.Failure($"Account '{account.AccountCode}' is currently inactive.");
            }
        }

        var invoice = new Invoice(
            request.InvoiceNumber,
            request.VendorName,
            request.IssueDate,
            request.DueDate,
            request.TaxAmount,
            request.TotalAmount);

        if (!string.IsNullOrWhiteSpace(request.UploadedFilePath) && !string.IsNullOrWhiteSpace(request.ContentType))
        {
            invoice.AttachDocument(request.UploadedFilePath, request.ContentType);
        }

        foreach (var itemDto in request.LineItems)
        {
            var line = new InvoiceLineItem(
                itemDto.AccountId,
                itemDto.Description,
                itemDto.Quantity,
                itemDto.UnitPrice);

            invoice.AddLineItem(line);
        }

        // 4. Save and return DTO.
        await _invoiceRepository.AddAsync(invoice, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<InvoiceDto>.Success(invoice.ToDto());
    }
}
