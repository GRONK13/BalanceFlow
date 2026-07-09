namespace BalanceFlow.Application.Interfaces;

/// <summary>
/// Service contract to simulate/execute OCR text extraction on invoice documents.
/// </summary>
public interface IOcrService
{
    Task<OcrExtractionResult> ExtractInvoiceAsync(
        Stream fileStream,
        string fileName,
        CancellationToken cancellationToken = default);
}
