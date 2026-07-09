namespace BalanceFlow.Application.Features.Invoices.Queries.GetInvoiceById;

/// <summary>
/// Handles <see cref="GetInvoiceByIdQuery"/>:
/// Fetches details and maps them to an InvoiceDto.
/// </summary>
public sealed class GetInvoiceByIdQueryHandler
    : IRequestHandler<GetInvoiceByIdQuery, Result<InvoiceDto>>
{
    private readonly IInvoiceRepository _invoiceRepository;

    public GetInvoiceByIdQueryHandler(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<Result<InvoiceDto>> Handle(
        GetInvoiceByIdQuery request,
        CancellationToken cancellationToken)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(request.Id, cancellationToken);
        if (invoice is null)
        {
            return Result<InvoiceDto>.Failure($"Invoice with ID '{request.Id}' was not found.");
        }

        return Result<InvoiceDto>.Success(invoice.ToDto());
    }
}
