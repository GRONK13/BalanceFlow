namespace BalanceFlow.UnitTests.Domain;

public sealed class JournalEntryLineTests
{
    [Fact]
    public void Constructor_WithBothDebitAndCreditNonZero_ShouldThrowArgumentException()
    {
        // Arrange
        var accountId = Guid.NewGuid();

        // Act
        var act = () => new JournalEntryLine(accountId, 100m, 50m);

        // Assert
        act.Should().ThrowExactly<ArgumentException>()
           .WithMessage("*cannot have both a debit and a credit amount*");
    }

    [Fact]
    public void Constructor_WithBothDebitAndCreditZero_ShouldThrowArgumentException()
    {
        // Arrange
        var accountId = Guid.NewGuid();

        // Act
        var act = () => new JournalEntryLine(accountId, 0m, 0m);

        // Assert
        act.Should().ThrowExactly<ArgumentException>()
           .WithMessage("*either a debit or a credit amount greater than zero*");
    }

    [Fact]
    public void Constructor_WithNegativeAmounts_ShouldThrowArgumentException()
    {
        // Arrange
        var accountId = Guid.NewGuid();

        // Act & Assert
        var actDebit = () => new JournalEntryLine(accountId, -100m, 0m);
        actDebit.Should().ThrowExactly<ArgumentException>().WithMessage("*Debit amount must not be negative*");

        var actCredit = () => new JournalEntryLine(accountId, 0m, -50m);
        actCredit.Should().ThrowExactly<ArgumentException>().WithMessage("*Credit amount must not be negative*");
    }
}
