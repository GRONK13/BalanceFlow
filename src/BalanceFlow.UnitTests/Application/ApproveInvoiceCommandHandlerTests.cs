using BalanceFlow.Application.Features.Invoices.Commands.ApproveInvoice;

namespace BalanceFlow.UnitTests.Application;

public sealed class ApproveInvoiceCommandHandlerTests
{
    private readonly IInvoiceRepository _invoiceRepositoryMock;
    private readonly IAccountRepository _accountRepositoryMock;
    private readonly IJournalEntryRepository _journalEntryRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly ApproveInvoiceCommandHandler _handler;

    public ApproveInvoiceCommandHandlerTests()
    {
        _invoiceRepositoryMock = Substitute.For<IInvoiceRepository>();
        _accountRepositoryMock = Substitute.For<IAccountRepository>();
        _journalEntryRepositoryMock = Substitute.For<IJournalEntryRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();

        _handler = new ApproveInvoiceCommandHandler(
            _invoiceRepositoryMock,
            _accountRepositoryMock,
            _journalEntryRepositoryMock,
            _unitOfWorkMock);
    }

    [Fact]
    public async Task Handle_WhenInvoiceDoesNotExist_ShouldReturnFailureResult()
    {
        // Arrange
        var command = new ApproveInvoiceCommand(Guid.NewGuid(), Guid.NewGuid());
        _invoiceRepositoryMock.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns((Invoice?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Should().Contain("not found");
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenAccountsPayableAccountDoesNotExist_ShouldReturnFailureResult()
    {
        // Arrange
        var invoice = new Invoice("INV-001", "Acme", DateTime.UtcNow, DateTime.UtcNow.AddDays(30), 0m, 100m);
        var command = new ApproveInvoiceCommand(invoice.Id, Guid.NewGuid());

        _invoiceRepositoryMock.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(invoice);
        _accountRepositoryMock.GetByIdAsync(command.AccountsPayableAccountId, Arg.Any<CancellationToken>())
            .Returns((Account?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Should().Contain("not found");
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenAPAccountIsNotLiability_ShouldReturnFailureResult()
    {
        // Arrange
        var invoice = new Invoice("INV-001", "Acme", DateTime.UtcNow, DateTime.UtcNow.AddDays(30), 0m, 100m);
        var assetAccount = new Account("1000", "Cash", AccountType.Asset); // Invariant check: AP account must be a liability!
        var command = new ApproveInvoiceCommand(invoice.Id, assetAccount.Id);

        _invoiceRepositoryMock.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(invoice);
        _accountRepositoryMock.GetByIdAsync(command.AccountsPayableAccountId, Arg.Any<CancellationToken>())
            .Returns(assetAccount);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Should().Contain("must be of classification Liability");
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenAPAccountIsValidAndInvoiceAudited_ShouldSucceedAndRegisterPosting()
    {
        // Arrange
        var invoice = new Invoice("INV-001", "Acme", DateTime.UtcNow, DateTime.UtcNow.AddDays(30), 10m, 110m);
        var expenseAccount = new Account("5000", "Supplies", AccountType.Expense);
        var line = new InvoiceLineItem(expenseAccount.Id, "Chairs", 1, 100m);
        invoice.AddLineItem(line);
        invoice.Audit(); // transitions status to Audited (Passed)

        var apAccount = new Account("2100", "Accounts Payable", AccountType.Liability);
        var command = new ApproveInvoiceCommand(invoice.Id, apAccount.Id);

        _invoiceRepositoryMock.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(invoice);
        _accountRepositoryMock.GetByIdAsync(command.AccountsPayableAccountId, Arg.Any<CancellationToken>())
            .Returns(apAccount);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Status.Should().Be(InvoiceStatus.Approved);

        // Verify ledger entries were posted and unit of work saved changes
        await _journalEntryRepositoryMock.Received(1).AddAsync(Arg.Is<JournalEntry>(j => j.ReferenceNumber == "AP-Acme-INV-001"), Arg.Any<CancellationToken>());
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
