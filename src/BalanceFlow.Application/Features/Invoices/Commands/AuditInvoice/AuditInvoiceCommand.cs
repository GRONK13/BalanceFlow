namespace BalanceFlow.Application.Features.Invoices.Commands.AuditInvoice;

/// <summary>
/// Command to run automated audit rules on a draft invoice.
/// </summary>
public sealed record AuditInvoiceCommand(Guid Id) : IRequest<Result<InvoiceDto>>;
