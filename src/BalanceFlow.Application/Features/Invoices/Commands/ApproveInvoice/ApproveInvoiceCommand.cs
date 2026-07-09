namespace BalanceFlow.Application.Features.Invoices.Commands.ApproveInvoice;

/// <summary>
/// Command to approve an audited invoice and generate a balanced double-entry
/// journal entry draft posted to the ledger.
/// </summary>
public sealed record ApproveInvoiceCommand(
    Guid Id,
    Guid AccountsPayableAccountId
) : IRequest<Result<InvoiceDto>>;
