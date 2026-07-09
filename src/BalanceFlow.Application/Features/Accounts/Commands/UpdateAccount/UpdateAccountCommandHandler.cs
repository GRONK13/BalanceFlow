namespace BalanceFlow.Application.Features.Accounts.Commands.UpdateAccount;

/// <summary>
/// Handles <see cref="UpdateAccountCommand"/>:
/// 1. Loads the existing account.
/// 2. Validates that the new account code is unique (if changed).
/// 3. Applies changes through domain methods (which re-validate invariants).
/// 4. Persists and returns the updated DTO.
/// </summary>
public sealed class UpdateAccountCommandHandler
    : IRequestHandler<UpdateAccountCommand, Result<AccountDto>>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAccountCommandHandler(IAccountRepository accountRepository, IUnitOfWork unitOfWork)
    {
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AccountDto>> Handle(
        UpdateAccountCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Load the existing account.
        var account = await _accountRepository.GetByIdAsync(request.Id, cancellationToken);

        if (account is null)
            return Result<AccountDto>.Failure(
                $"Account with ID '{request.Id}' was not found.");

        // 2. If the account code is changing, verify the new code is unique.
        if (!string.Equals(account.AccountCode, request.AccountCode, StringComparison.OrdinalIgnoreCase))
        {
            var existing = await _accountRepository.GetByAccountCodeAsync(
                request.AccountCode, cancellationToken);

            if (existing is not null)
                return Result<AccountDto>.Failure(
                    $"An account with code '{request.AccountCode}' already exists.");
        }

        // 3. Apply changes through domain methods (domain re-validates invariants).
        account.SetAccountCode(request.AccountCode);
        account.SetName(request.Name);
        account.SetDescription(request.Description);

        // 4. Persist and return.
        _accountRepository.Update(account);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<AccountDto>.Success(account.ToDto());
    }
}
