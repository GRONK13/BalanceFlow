namespace BalanceFlow.UnitTests.Domain;

public sealed class InvoiceTests
{
    [Fact]
    public void Audit_WithMatchingTotalsAndValidDates_ShouldPassAudit()
    {
        // Arrange
        var invoice = new Invoice("INV-001", "Acme Corp", DateTime.UtcNow, DateTime.UtcNow.AddDays(30), 10m, 110m);
        var expenseAccount = new Account("5000", "Office Expense", AccountType.Expense);
        
        var line = new InvoiceLineItem(expenseAccount.Id, "Supplies", 1, 100m);
        invoice.AddLineItem(line);

        // Act
        invoice.Audit();

        // Assert
        invoice.AuditStatus.Should().Be(InvoiceAuditStatus.Passed);
        invoice.Status.Should().Be(InvoiceStatus.Audited);
        invoice.AuditNotes.Should().Contain("checks passed");
    }

    [Fact]
    public void Audit_WithMismatchedTotals_ShouldFailAudit()
    {
        // Arrange
        // Total is declared as 120m, but line item total ($100) + tax ($10) = $110. (10m difference)
        var invoice = new Invoice("INV-001", "Acme Corp", DateTime.UtcNow, DateTime.UtcNow.AddDays(30), 10m, 120m);
        var expenseAccount = new Account("5000", "Office Expense", AccountType.Expense);
        
        var line = new InvoiceLineItem(expenseAccount.Id, "Supplies", 1, 100m);
        invoice.AddLineItem(line);

        // Act
        invoice.Audit();

        // Assert
        invoice.AuditStatus.Should().Be(InvoiceAuditStatus.Failed);
        invoice.Status.Should().Be(InvoiceStatus.Rejected);
        invoice.AuditNotes.Should().Contain("Mathematical Discrepancy");
    }

    [Fact]
    public void Approve_WhenInvoiceIsNotPassedOrAudited_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var invoice = new Invoice("INV-001", "Acme Corp", DateTime.UtcNow, DateTime.UtcNow.AddDays(30), 10m, 110m);
        var apAccountId = Guid.NewGuid();

        // Act
        var act = () => invoice.Approve(apAccountId);

        // Assert
        act.Should().ThrowExactly<InvalidOperationException>()
           .WithMessage("*Only audited invoices that have passed validation can be approved*");
    }

    [Fact]
    public void Approve_WhenAuditPassed_ShouldGenerateBalancedLedgerEntryAndSetApproved()
    {
        // Arrange
        var invoice = new Invoice("INV-001", "Acme Corp", DateTime.UtcNow, DateTime.UtcNow.AddDays(30), 10m, 110m);
        var expenseAccount = new Account("5000", "Office Expense", AccountType.Expense);
        
        var line = new InvoiceLineItem(expenseAccount.Id, "Supplies", 1, 100m);
        invoice.AddLineItem(line);
        invoice.Audit();

        var apAccountId = Guid.NewGuid();

        // Act
        var entry = invoice.Approve(apAccountId);

        // Assert
        invoice.Status.Should().Be(InvoiceStatus.Approved);
        entry.Should().NotBeNull();
        entry.ReferenceNumber.Should().Be("AP-AcmeCorp-INV-001");
        
        // Ledger entry lines checks
        entry.Lines.Should().HaveCount(2);
        
        var debitLine = entry.Lines.First(l => l.DebitAmount > 0);
        debitLine.AccountId.Should().Be(expenseAccount.Id);
        debitLine.DebitAmount.Should().Be(110m);

        var creditLine = entry.Lines.First(l => l.CreditAmount > 0);
        creditLine.AccountId.Should().Be(apAccountId);
        creditLine.CreditAmount.Should().Be(110m); // Matches invoice total

        // In a true double-entry system, our generated AP entry balances debits/credits!
        // Note: For this simple demonstration audit mapping, the line items sum to $100, while AP is $110 credit.
        // Wait! A balanced journal entry requires total debits to equal total credits.
        // In our Invoice.Approve logic, the sum of lines total ($100) does NOT equal AP credit ($110) because of the $10 Tax.
        // In a real-world accounting system, the Tax is mapped to a Tax Expense/Asset account as a debit (e.g. Sales Tax Paid).
        // Let's look at our Invoice.Approve code to see how it balances:
        // In our current Invoice.Approve implementation:
        // It debits line items ($100), credits AP ($110). That is UNBALANCED (TotalDebits = 100, TotalCredits = 110).
        // If we try to call Post() on this generated JournalEntry, it will throw UnbalancedJournalEntryException!
        // Wait, is that true? Let's check how the test behaves. Let's write the test so that it checks if entry is balanced.
        // Wait! In the code we wrote:
        // ```csharp
        // foreach (var line in _lineItems)
        // {
        //     var debitLine = new JournalEntryLine(line.AccountId, line.LineTotal, 0, line.Description);
        //     journalEntry.AddLine(debitLine);
        // }
        // var creditLine = new JournalEntryLine(accountsPayableAccountId, 0, TotalAmount, ...);
        // ```
        // TotalAmount = LineTotalSum + TaxAmount.
        // So the debits are `LineTotalSum`, and the credit is `LineTotalSum + TaxAmount`.
        // This means it has a discrepancy of `TaxAmount`!
        // Wait! If `TaxAmount == 0`, then it is balanced.
        // If `TaxAmount > 0`, it will NOT be balanced unless we add a tax line.
        // Oh! That is a bug in the domain integration logic! The ledger entry generated from `Approve` will fail validation (Post()) if `TaxAmount > 0` because there is no line for the tax debit.
        // Let's verify: In double entry, the tax must be debited to a tax account (e.g. GST/VAT input credit or tax expense) or added as part of the line items cost.
        // If the tax is bundled into the lines, then `TotalAmount` equals `Sum(LineTotals)`.
        // If we want a separate tax debit, we should have a `TaxAccountId` configuration, or bundle tax into the expense lines.
        // In many simple micro-accounting systems, the tax is simply added to the expense line or posted to a single default Tax Expense account.
        // Let's check if we can adjust the `Approve` method in `Invoice.cs` to bundle tax into the line items proportionately, OR map it to a default tax account, or simply have the line item totals include tax, or add a tax line.
        // Wait, the simplest way to ensure a balanced entry is: if `TaxAmount > 0`, we add a tax debit line. To do that, we need a target tax account!
        // Or we can distribute the tax proportionately to each line's expense account! That is very common when tax is capitalized.
        // Let's look at the implementation of `Approve` in `Invoice.cs` that we wrote:
        // It adds debits for `line.LineTotal`, and credits `TotalAmount`.
        // If `TaxAmount > 0`, it's unbalanced. Let's fix this in `Invoice.cs`!
        // We can either:
        // A) Distribute the tax proportionately to the debit lines: `line.LineTotal + (line.LineTotal / Sum(LineTotals) * TaxAmount)`.
        // B) Or pass a `taxAccountId` to `Approve` and create a debit line for tax.
        // Let's look at what is cleaner. Capitalizing tax (distributing it proportionately to the lines' accounts) is extremely simple and ensures that we don't need another configuration parameter.
        // Or we can pass a `taxAccountId` to `Approve(accountsPayableAccountId, taxAccountId)`.
        // Let's check: if we distribute the tax proportionately, the math is:
        // `debitAmount = line.LineTotal + (line.LineTotal / SumOfLines) * TaxAmount`.
        // Wait! If `SumOfLines` is zero, we don't approve (but audit checks for at least one line item, so it's not zero).
        // Let's implement capitalization in `Invoice.cs`! It makes the entry balance perfectly.
        // Let's see: `debitAmount = line.LineTotal + (line.LineTotal / sumOfLines) * TaxAmount`.
        // Wait, let's verify if there could be a rounding issue. To avoid rounding issues, the last line item can take the remainder:
        // `var lineTax = Math.Round((line.LineTotal / sumOfLines) * TaxAmount, 2);`
        // We can write a clean, balanced mapping!
        // Let's check: if we distribute tax, then the sum of debits will exactly equal `TotalAmount`, which is `Sum(LineTotal) + TaxAmount`.
        // Let's do this by modifying `Invoice.cs` first to make it bulletproof and professional!
    }
}
