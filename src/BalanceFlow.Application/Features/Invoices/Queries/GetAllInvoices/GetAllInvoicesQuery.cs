namespace BalanceFlow.Application.Features.Invoices.Queries.GetAllInvoices;

/// <summary>
/// Query to retrieve a paginated listing of invoices.
/// </summary>
public sealed record GetAllInvoicesQuery(
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<Result<PagedResult<InvoiceDto>>>;
