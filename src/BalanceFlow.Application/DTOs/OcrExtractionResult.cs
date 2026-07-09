namespace BalanceFlow.Application.DTOs;

/// <summary>
/// DTO containing extracted fields from a document OCR extraction scan.
/// </summary>
public sealed record OcrExtractionResult(
    string InvoiceNumber,
    string VendorName,
    DateTime? IssueDate,
    DateTime? DueDate,
    decimal TaxAmount,
    decimal TotalAmount,
    string UploadedFilePath,
    string ContentType,
    IReadOnlyList<ExtractedLineItemDto> LineItems
);
