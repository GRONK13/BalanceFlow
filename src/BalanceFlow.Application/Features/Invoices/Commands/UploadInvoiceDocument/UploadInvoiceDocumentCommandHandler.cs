namespace BalanceFlow.Application.Features.Invoices.Commands.UploadInvoiceDocument;

/// <summary>
/// Handles <see cref="UploadInvoiceDocumentCommand"/>:
/// 1. Saves the uploaded file stream into the local "uploads" directory.
/// 2. Executes the mock OCR extraction service.
/// 3. Returns the extracted headers, itemized lines, and file metadata.
/// </summary>
public sealed class UploadInvoiceDocumentCommandHandler
    : IRequestHandler<UploadInvoiceDocumentCommand, Result<OcrExtractionResult>>
{
    private readonly IOcrService _ocrService;

    public UploadInvoiceDocumentCommandHandler(IOcrService ocrService)
    {
        _ocrService = ocrService;
    }

    public async Task<Result<OcrExtractionResult>> Handle(
        UploadInvoiceDocumentCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // 1. Establish the local storage folder (relative uploads/ directory)
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Generate a unique filename to avoid duplicates
            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(request.FileName)}";
            var physicalPath = Path.Combine(uploadsFolder, uniqueFileName);

            // Save stream to physical file on disk
            using (var fileStream = new FileStream(physicalPath, FileMode.Create, FileAccess.Write))
            {
                await request.FileStream.CopyToAsync(fileStream, cancellationToken);
            }

            // Define the relative web/storage path to return
            var relativeFilePath = Path.Combine("uploads", uniqueFileName).Replace('\\', '/');

            // 2. Seek to beginning of stream before passing to OCR service
            request.FileStream.Position = 0;

            // 3. Trigger mock OCR extraction
            var extraction = await _ocrService.ExtractInvoiceAsync(
                request.FileStream,
                request.FileName,
                cancellationToken);

            // 4. Return extraction data enriched with the saved document path coordinates
            var result = new OcrExtractionResult(
                InvoiceNumber: extraction.InvoiceNumber,
                VendorName: extraction.VendorName,
                IssueDate: extraction.IssueDate,
                DueDate: extraction.DueDate,
                TaxAmount: extraction.TaxAmount,
                TotalAmount: extraction.TotalAmount,
                UploadedFilePath: relativeFilePath,
                ContentType: request.ContentType,
                LineItems: extraction.LineItems);

            return Result<OcrExtractionResult>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<OcrExtractionResult>.Failure($"Failed to process document upload: {ex.Message}");
        }
    }
}
