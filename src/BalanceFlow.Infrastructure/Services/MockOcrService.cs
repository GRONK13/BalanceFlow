using BalanceFlow.Application.Interfaces;
using BalanceFlow.Application.DTOs;

namespace BalanceFlow.Infrastructure.Services;

/// <summary>
/// Mock implementation of <see cref="IOcrService"/> for portfolio demonstration.
/// Analyzes the uploaded file name to dynamically extract context-aware vendor headers
/// and suggested ledger accounts from the database to simulate real AI document extraction.
/// </summary>
public sealed class MockOcrService : IOcrService
{
    private readonly ApplicationDbContext _dbContext;

    public MockOcrService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OcrExtractionResult> ExtractInvoiceAsync(
        Stream fileStream,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        // Simulate minor processing delay for realistic UX in frontend
        await Task.Delay(800, cancellationToken);

        var nameLower = fileName.ToLowerInvariant();

        // 1. Context: AWS Hosting
        if (nameLower.Contains("aws") || nameLower.Contains("amazon") || nameLower.Contains("cloud"))
        {
            var suggestedAccount = await FindAccountByNameAsync("utilities", "hosting", "cloud", "server");

            return new OcrExtractionResult(
                InvoiceNumber: $"AWS-{DateTime.UtcNow:yyyyMMdd}-894",
                VendorName: "Amazon Web Services, Inc.",
                IssueDate: DateTime.UtcNow.AddDays(-2),
                DueDate: DateTime.UtcNow.AddDays(12),
                TaxAmount: 18.00m,
                TotalAmount: 118.00m,
                UploadedFilePath: string.Empty, // Filled by command handler
                ContentType: string.Empty,      // Filled by command handler
                LineItems: new List<ExtractedLineItemDto>
                {
                    new("EC2 Computing Instance Reservation (m5.large)", 1, 80.00m, suggestedAccount?.Id),
                    new("S3 Simple Storage Standard Tier", 1, 20.00m, suggestedAccount?.Id)
                }
            );
        }

        // 2. Context: Office Supplies (Staples)
        if (nameLower.Contains("staples") || nameLower.Contains("office") || nameLower.Contains("supplies"))
        {
            var suggestedAccount = await FindAccountByNameAsync("office", "supplies", "stationery", "printing");

            return new OcrExtractionResult(
                InvoiceNumber: $"STPL-{DateTime.UtcNow:yyyyMMdd}-92",
                VendorName: "Staples Business Delivery",
                IssueDate: DateTime.UtcNow.AddDays(-1),
                DueDate: DateTime.UtcNow.AddDays(14),
                TaxAmount: 4.50m,
                TotalAmount: 54.50m,
                UploadedFilePath: string.Empty,
                ContentType: string.Empty,
                LineItems: new List<ExtractedLineItemDto>
                {
                    new("Premium Copy Paper - Letter (Case)", 1, 40.00m, suggestedAccount?.Id),
                    new("Retractable Gel Pens (Dozen)", 1, 10.00m, suggestedAccount?.Id)
                }
            );
        }

        // 3. Context: Default general invoice fallback
        var defaultAccount = await _dbContext.Accounts
            .FirstOrDefaultAsync(a => a.IsActive && a.Type == AccountType.Expense, cancellationToken);

        return new OcrExtractionResult(
            InvoiceNumber: $"INV-{DateTime.UtcNow:yyyyMMdd}-001",
            VendorName: "General Vendor Solutions Ltd.",
            IssueDate: DateTime.UtcNow,
            DueDate: DateTime.UtcNow.AddDays(30),
            TaxAmount: 0.00m,
            TotalAmount: 100.00m,
            UploadedFilePath: string.Empty,
            ContentType: string.Empty,
            LineItems: new List<ExtractedLineItemDto>
            {
                new("Business Consulting Services", 1, 100.00m, defaultAccount?.Id)
            }
        );
    }

    private async Task<Account?> FindAccountByNameAsync(params string[] keywords)
    {
        foreach (var keyword in keywords)
        {
            var account = await _dbContext.Accounts
                .FirstOrDefaultAsync(a => a.IsActive && a.Name.ToLower().Contains(keyword));
            
            if (account is not null)
            {
                return account;
            }
        }

        // Fallback to first active expense account if no keywords matched
        return await _dbContext.Accounts
            .FirstOrDefaultAsync(a => a.IsActive && a.Type == AccountType.Expense);
    }
}
