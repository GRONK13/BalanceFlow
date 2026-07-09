using BalanceFlow.Application.Features.Accounts.Commands.CreateAccount;

namespace BalanceFlow.UnitTests.Application;

public sealed class CreateAccountCommandHandlerTests
{
    private readonly IAccountRepository _accountRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly CreateAccountCommandHandler _handler;

    public CreateAccountCommandHandlerTests()
    {
        _accountRepositoryMock = Substitute.For<IAccountRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _handler = new CreateAccountCommandHandler(_accountRepositoryMock, _unitOfWorkMock);
    }

    [Fact]
    public async Task Handle_WhenAccountCodeIsUnique_ShouldCreateAndSaveAccount()
    {
        // Arrange
        var command = new CreateAccountCommand("1000", "Cash", AccountType.Asset, "Main bank cash account");
        
        _accountRepositoryMock.GetByAccountCodeAsync(command.AccountCode, Arg.Any<CancellationToken>())
            .Returns((Account?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.AccountCode.Should().Be("1000");
        result.Data!.Name.Should().Be("Cash");

        // Verify repository interaction
        await _accountRepositoryMock.Received(1).AddAsync(Arg.Is<Account>(a => a.AccountCode == "1000"), Arg.Any<CancellationToken>());
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenAccountCodeAlreadyExists_ShouldReturnFailureResult()
    {
        // Arrange
        var command = new CreateAccountCommand("1000", "Cash", AccountType.Asset, "Main bank cash account");
        var existingAccount = new Account("1000", "Cash", AccountType.Asset);

        _accountRepositoryMock.GetByAccountCodeAsync(command.AccountCode, Arg.Any<CancellationToken>())
            .Returns(existingAccount);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Should().Contain("already exists");

        // Verify repository interactions: AddAsync should NEVER be called
        await _accountRepositoryMock.DidNotReceive().AddAsync(Arg.Any<Account>(), Arg.Any<CancellationToken>());
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
