namespace BalanceFlow.Application.Features.Invoices.Commands.CreateInvoice;

/// <summary>
/// Command to record a new invoice in Draft state.
/// </summary>
public sealed record CreateInvoiceCommand(
    string InvoiceNumber,
    string VendorName,
    DateTime IssueDate,
    DateTime DueDate,
    decimal TaxAmount,
    decimal TotalAmount,
    List<CreateInvoiceLineItemDto> LineItems,
    string? UploadedFilePath = null,
    string? ContentType = null
) : IRequest<Result<InvoiceDto>>;

/// <summary>
/// DTO representing line item details in a CreateInvoiceCommand.
/// </summary>
public sealed record CreateInvoiceLineItemDto(
    Guid AccountId,
    string Description,
    int Quantity,
    decimal UnitPrice
);
