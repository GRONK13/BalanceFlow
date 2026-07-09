namespace BalanceFlow.Application.Features.Invoices.Queries.GetInvoiceById;

/// <summary>
/// Query to retrieve a single invoice by its Guid identifier.
/// </summary>
public sealed record GetInvoiceByIdQuery(Guid Id) : IRequest<Result<InvoiceDto>>;
