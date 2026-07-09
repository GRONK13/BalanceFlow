namespace BalanceFlow.Application.Features.Invoices.Queries.GetAllInvoices;

/// <summary>
/// Handles <see cref="GetAllInvoicesQuery"/>:
/// Fetches the current slice of invoices, maps them, and returns paginated metadata.
/// </summary>
public sealed class GetAllInvoicesQueryHandler
    : IRequestHandler<GetAllInvoicesQuery, Result<PagedResult<InvoiceDto>>>
{
    private readonly IInvoiceRepository _invoiceRepository;

    public GetAllInvoicesQueryHandler(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<Result<PagedResult<InvoiceDto>>> Handle(
        GetAllInvoicesQuery request,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _invoiceRepository.GetAllAsync(
            request.PageNumber, request.PageSize, cancellationToken);

        var dtos = items.Select(i => i.ToDto()).ToList();

        var pagedResult = new PagedResult<InvoiceDto>(
            dtos, totalCount, request.PageNumber, request.PageSize);

        return Result<PagedResult<InvoiceDto>>.Success(pagedResult);
    }
}
