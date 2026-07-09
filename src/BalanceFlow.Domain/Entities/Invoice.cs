namespace BalanceFlow.Domain.Entities;

/// <summary>
/// Represents a Vendor Invoice in the system. 
/// Owns list items and acts as the aggregate root for invoice audits and ledger postings.
/// </summary>
public sealed class Invoice : BaseEntity
{
    /// <summary>Unique, sequential invoice identifier provided by the vendor.</summary>
    public string InvoiceNumber { get; private set; } = string.Empty;

    /// <summary>Vendor name.</summary>
    public string VendorName { get; private set; } = string.Empty;

    /// <summary>Date when the invoice was issued by the vendor.</summary>
    public DateTime IssueDate { get; private set; }

    /// <summary>Due date for payment.</summary>
    public DateTime DueDate { get; private set; }

    /// <summary>Sales tax amount applied to this invoice.</summary>
    public decimal TaxAmount { get; private set; }

    /// <summary>Total amount of the invoice (inclusive of line items and tax).</summary>
    public decimal TotalAmount { get; private set; }

    /// <summary>Workflow status of the invoice.</summary>
    public InvoiceStatus Status { get; private set; } = InvoiceStatus.Draft;

    /// <summary>Compliance audit status of the invoice.</summary>
    public InvoiceAuditStatus AuditStatus { get; private set; } = InvoiceAuditStatus.NotAudited;

    /// <summary>System validation log messages or audit failure notes.</summary>
    public string? AuditNotes { get; private set; }

    /// <summary>Relative physical path to the stored invoice document (PDF/Image).</summary>
    public string? UploadedFilePath { get; private set; }

    /// <summary>MIME Content-Type of the uploaded document.</summary>
    public string? ContentType { get; private set; }

    // Private backing collection for lines
    private readonly List<InvoiceLineItem> _lineItems = [];

    /// <summary>Read-only collection of itemized invoice lines.</summary>
    public IReadOnlyCollection<InvoiceLineItem> LineItems => _lineItems.AsReadOnly();

    // EF Core constructor
    private Invoice() { }

    public Invoice(
        string invoiceNumber,
        string vendorName,
        DateTime issueDate,
        DateTime dueDate,
        decimal taxAmount,
        decimal totalAmount)
    {
        SetInvoiceNumber(invoiceNumber);
        SetVendorName(vendorName);
        
        if (dueDate < issueDate)
            throw new ArgumentException("Due date cannot be earlier than the issue date.", nameof(dueDate));

        if (taxAmount < 0)
            throw new ArgumentException("Tax amount cannot be negative.", nameof(taxAmount));

        if (totalAmount <= 0)
            throw new ArgumentException("Total amount must be greater than zero.", nameof(totalAmount));

        IssueDate = issueDate;
        DueDate = dueDate;
        TaxAmount = taxAmount;
        TotalAmount = totalAmount;
    }

    public void SetInvoiceNumber(string invoiceNumber)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(invoiceNumber, nameof(invoiceNumber));
        EnsureEditable();
        InvoiceNumber = invoiceNumber.Trim();
    }

    public void SetVendorName(string vendorName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(vendorName, nameof(vendorName));
        EnsureEditable();
        VendorName = vendorName.Trim();
    }

    public void AddLineItem(InvoiceLineItem line)
    {
        ArgumentNullException.ThrowIfNull(line, nameof(line));
        EnsureEditable();
        _lineItems.Add(line);
    }

    /// <summary>
    /// Associates a physical uploaded document with this invoice.
    /// </summary>
    public void AttachDocument(string uploadedFilePath, string contentType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(uploadedFilePath, nameof(uploadedFilePath));
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType, nameof(contentType));
        EnsureEditable();

        UploadedFilePath = uploadedFilePath.Trim();
        ContentType = contentType.Trim();
    }

    /// <summary>
    /// Executes automated audit rules on this invoice.
    /// Checks for mathematical alignment and logical rules.
    /// </summary>
    public void Audit()
    {
        EnsureEditable();

        var sumOfLines = _lineItems.Sum(l => l.LineTotal);
        var expectedTotal = sumOfLines + TaxAmount;

        var notes = new List<string>();

        if (Math.Abs(expectedTotal - TotalAmount) > 0.01m)
        {
            notes.Add($"Mathematical Discrepancy: Line items total sum ($" +
                      $"{sumOfLines:F2}) + Tax ($" +
                      $"{TaxAmount:F2}) equals ${expectedTotal:F2}, " +
                      $"but invoice total is listed as ${TotalAmount:F2}.");
        }

        if (DueDate < IssueDate)
        {
            notes.Add("Logical Error: Due date is earlier than the issue date.");
        }

        if (_lineItems.Count == 0)
        {
            notes.Add("Validation Error: Invoice must contain at least one line item.");
        }

        if (notes.Count > 0)
        {
            AuditStatus = InvoiceAuditStatus.Failed;
            Status = InvoiceStatus.Rejected;
            AuditNotes = string.Join(" | ", notes);
        }
        else
        {
            AuditStatus = InvoiceAuditStatus.Passed;
            Status = InvoiceStatus.Audited;
            AuditNotes = "Automated audit checks passed successfully.";
        }
    }

    /// <summary>
    /// Approves the invoice and maps its details directly to a balanced double-entry
    /// <see cref="JournalEntry"/> draft to post to the ledger.
    /// </summary>
    /// <param name="accountsPayableAccountId">The liability account ID representing Accounts Payable.</param>
    /// <returns>A balanced JournalEntry aggregate root.</returns>
    public JournalEntry Approve(Guid accountsPayableAccountId)
    {
        if (Status != InvoiceStatus.Audited || AuditStatus != InvoiceAuditStatus.Passed)
        {
            throw new InvalidOperationException("Only audited invoices that have passed validation can be approved.");
        }

        Status = InvoiceStatus.Approved;
        AuditNotes = $"Approved and ledger entry drafted on {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC.";

        // Create the journal entry reference. e.g. "INV-AUDIT-[Vendor]-[InvoiceNumber]"
        var reference = $"AP-{VendorName.Replace(" ", "")}-{InvoiceNumber}";
        if (reference.Length > 50)
        {
            reference = reference[..50];
        }

        var journalEntry = new JournalEntry(reference, IssueDate, $"Audited invoice posting for {VendorName}. Invoice #{InvoiceNumber}");

        var sumOfLines = _lineItems.Sum(l => l.LineTotal);
        var remainingTax = TaxAmount;

        // Add expense/asset debits based on line items, distributing tax proportionately
        for (int i = 0; i < _lineItems.Count; i++)
        {
            var line = _lineItems[i];
            decimal lineTax;

            if (i == _lineItems.Count - 1)
            {
                // Prevents rounding discrepancies by allocating the exact remaining tax to the last item
                lineTax = remainingTax;
            }
            else
            {
                lineTax = Math.Round((line.LineTotal / sumOfLines) * TaxAmount, 2);
                remainingTax -= lineTax;
            }

            var debitAmount = line.LineTotal + lineTax;
            var debitLine = new JournalEntryLine(line.AccountId, debitAmount, 0, line.Description);
            journalEntry.AddLine(debitLine);
        }

        // Add accounts payable credit (liability posting)
        var creditLine = new JournalEntryLine(accountsPayableAccountId, 0, TotalAmount, $"Accounts Payable credit for {VendorName}");
        journalEntry.AddLine(creditLine);

        return journalEntry;
    }

    private void EnsureEditable()
    {
        if (Status == InvoiceStatus.Approved || Status == InvoiceStatus.Rejected)
        {
            throw new InvalidOperationException($"The invoice is in '{Status}' state and cannot be modified.");
        }
    }
}
