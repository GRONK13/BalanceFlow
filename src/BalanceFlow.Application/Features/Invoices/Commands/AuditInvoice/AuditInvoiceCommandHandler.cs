namespace BalanceFlow.Application.Features.Invoices.Commands.AuditInvoice;

/// <summary>
/// Handles <see cref="AuditInvoiceCommand"/>:
/// 1. Loads the invoice with its lines.
/// 2. Executes the domain's Audit() checks.
/// 3. Persists the audit results and returns the updated DTO.
/// </summary>
public sealed class AuditInvoiceCommandHandler
    : IRequestHandler<AuditInvoiceCommand, Result<InvoiceDto>>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AuditInvoiceCommandHandler(IInvoiceRepository invoiceRepository, IUnitOfWork unitOfWork)
    {
        _invoiceRepository = invoiceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<InvoiceDto>> Handle(
        AuditInvoiceCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Fetch the invoice
        var invoice = await _invoiceRepository.GetByIdAsync(request.Id, cancellationToken);
        if (invoice is null)
        {
            return Result<InvoiceDto>.Failure($"Invoice with ID '{request.Id}' was not found.");
        }

        // 2. Perform domain compliance audit checks
        try
        {
            invoice.Audit();
        }
        catch (InvalidOperationException ex)
        {
            return Result<InvoiceDto>.Failure(ex.Message);
        }

        // 3. Update database state and save
        _invoiceRepository.Update(invoice);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<InvoiceDto>.Success(invoice.ToDto());
    }
}
