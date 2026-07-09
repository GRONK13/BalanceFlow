namespace BalanceFlow.Application.DTOs;

/// <summary>
/// DTO representing an Invoice.
/// </summary>
public sealed record InvoiceDto(
    Guid Id,
    string InvoiceNumber,
    string VendorName,
    DateTime IssueDate,
    DateTime DueDate,
    decimal TaxAmount,
    decimal TotalAmount,
    InvoiceStatus Status,
    string StatusName,
    InvoiceAuditStatus AuditStatus,
    string AuditStatusName,
    string? AuditNotes,
    string? UploadedFilePath,
    string? ContentType,
    IReadOnlyList<InvoiceLineItemDto> LineItems,
    DateTime CreatedAt,
    DateTime? ModifiedAt
);
