namespace BalanceFlow.Application.Features.Invoices.Commands.UploadInvoiceDocument;

/// <summary>
/// Command to upload a physical invoice file, save it, and run mock OCR extraction.
/// </summary>
public sealed record UploadInvoiceDocumentCommand(
    Stream FileStream,
    string FileName,
    string ContentType
) : IRequest<Result<OcrExtractionResult>>;
