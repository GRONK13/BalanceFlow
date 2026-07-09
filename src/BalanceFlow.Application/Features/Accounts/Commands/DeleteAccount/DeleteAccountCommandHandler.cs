namespace BalanceFlow.Application.Features.Accounts.Commands.DeleteAccount;

/// <summary>
/// Handles <see cref="DeleteAccountCommand"/> by soft-deleting the account
/// (setting <c>IsDeleted = true</c>) rather than physically removing it.
/// This preserves historical data for audit trails and reporting.
/// </summary>
public sealed class DeleteAccountCommandHandler
    : IRequestHandler<DeleteAccountCommand, Result>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteAccountCommandHandler(IAccountRepository accountRepository, IUnitOfWork unitOfWork)
    {
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        DeleteAccountCommand request,
        CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdAsync(request.Id, cancellationToken);

        if (account is null)
            return Result.Failure($"Account with ID '{request.Id}' was not found.");

        // Soft-delete via the domain method (sets IsDeleted = true + updates ModifiedAt).
        account.SoftDelete();

        _accountRepository.Update(account);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
