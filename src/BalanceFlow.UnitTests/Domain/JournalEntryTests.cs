namespace BalanceFlow.UnitTests.Domain;

public sealed class JournalEntryTests
{
    [Fact]
    public void Post_WithLessContainingThanTwoLines_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var entry = new JournalEntry("JE-001", DateTime.UtcNow);
        var cashAccount = new Account("1000", "Cash", AccountType.Asset);
        var line = new JournalEntryLine(cashAccount.Id, 100m, 0);
        entry.AddLine(line);

        // Act
        var act = () => entry.Post();

        // Assert
        act.Should().ThrowExactly<InvalidOperationException>()
           .WithMessage("*must have at least 2 lines*");
    }

    [Fact]
    public void Post_WithUnbalancedDebitsAndCredits_ShouldThrowUnbalancedJournalEntryException()
    {
        // Arrange
        var entry = new JournalEntry("JE-001", DateTime.UtcNow);
        var assetAccount = new Account("1000", "Cash", AccountType.Asset);
        var expenseAccount = new Account("5000", "Rent", AccountType.Expense);

        var line1 = new JournalEntryLine(assetAccount.Id, 100m, 0);
        var line2 = new JournalEntryLine(expenseAccount.Id, 0, 90m); // 10m discrepancy

        entry.AddLine(line1);
        entry.AddLine(line2);

        // Act
        var act = () => entry.Post();

        // Assert
        act.Should().ThrowExactly<UnbalancedJournalEntryException>()
           .Which.Difference.Should().Be(10m);
    }

    [Fact]
    public void Post_WithBalancedDebitsAndCredits_ShouldSucceedAndMarkAsPosted()
    {
        // Arrange
        var entry = new JournalEntry("JE-001", DateTime.UtcNow);
        var assetAccount = new Account("1000", "Cash", AccountType.Asset);
        var revenueAccount = new Account("4000", "Sales", AccountType.Revenue);

        var line1 = new JournalEntryLine(assetAccount.Id, 100m, 0);
        var line2 = new JournalEntryLine(revenueAccount.Id, 0, 100m);

        entry.AddLine(line1);
        entry.AddLine(line2);

        // Act
        entry.Post();

        // Assert
        entry.IsPosted.Should().BeTrue();
        entry.PostedAt.Should().NotBeNull();
        entry.PostedAt!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Modifications_AfterPost_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var entry = new JournalEntry("JE-001", DateTime.UtcNow);
        var assetAccount = new Account("1000", "Cash", AccountType.Asset);
        var revenueAccount = new Account("4000", "Sales", AccountType.Revenue);

        var line1 = new JournalEntryLine(assetAccount.Id, 100m, 0);
        var line2 = new JournalEntryLine(revenueAccount.Id, 0, 100m);

        entry.AddLine(line1);
        entry.AddLine(line2);
        entry.Post();

        var extraLine = new JournalEntryLine(assetAccount.Id, 50m, 0);

        // Act & Assert
        var actAdd = () => entry.AddLine(extraLine);
        actAdd.Should().ThrowExactly<InvalidOperationException>().WithMessage("*already been posted*");

        var actRemove = () => entry.RemoveLine(line1.Id);
        actRemove.Should().ThrowExactly<InvalidOperationException>().WithMessage("*already been posted*");

        var actSetRef = () => entry.SetReferenceNumber("NEW-REF");
        actSetRef.Should().ThrowExactly<InvalidOperationException>().WithMessage("*already been posted*");
    }
}
