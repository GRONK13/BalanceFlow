namespace BalanceFlow.Application.Mappings;

/// <summary>
/// Extension methods for mapping Invoice domain models to DTOs.
/// </summary>
public static class InvoiceMappingExtensions
{
    public static InvoiceDto ToDto(this Invoice invoice) => new(
        Id: invoice.Id,
        InvoiceNumber: invoice.InvoiceNumber,
        VendorName: invoice.VendorName,
        IssueDate: invoice.IssueDate,
        DueDate: invoice.DueDate,
        TaxAmount: invoice.TaxAmount,
        TotalAmount: invoice.TotalAmount,
        Status: invoice.Status,
        StatusName: invoice.Status.ToString(),
        AuditStatus: invoice.AuditStatus,
        AuditStatusName: invoice.AuditStatus.ToString(),
        AuditNotes: invoice.AuditNotes,
        UploadedFilePath: invoice.UploadedFilePath,
        ContentType: invoice.ContentType,
        LineItems: invoice.LineItems.Select(l => l.ToDto()).ToList(),
        CreatedAt: invoice.CreatedAt,
        ModifiedAt: invoice.ModifiedAt
    );

    public static InvoiceLineItemDto ToDto(this InvoiceLineItem line) => new(
        Id: line.Id,
        AccountId: line.AccountId,
        AccountCode: line.Account?.AccountCode,
        AccountName: line.Account?.Name,
        Description: line.Description,
        Quantity: line.Quantity,
        UnitPrice: line.UnitPrice,
        LineTotal: line.LineTotal
    );
}
