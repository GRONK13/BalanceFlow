namespace BalanceFlow.Application.Features.Accounts.Queries.GetAccountById;

/// <summary>
/// Handles <see cref="GetAccountByIdQuery"/> — loads the account and maps to DTO.
/// Query handlers are simpler than command handlers because they never modify data.
/// Notice this handler only depends on the repository (no <see cref="IUnitOfWork"/>
/// needed since we're not writing anything).
/// </summary>
public sealed class GetAccountByIdQueryHandler
    : IRequestHandler<GetAccountByIdQuery, Result<AccountDto>>
{
    private readonly IAccountRepository _accountRepository;

    public GetAccountByIdQueryHandler(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task<Result<AccountDto>> Handle(
        GetAccountByIdQuery request,
        CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdAsync(request.Id, cancellationToken);

        if (account is null)
            return Result<AccountDto>.Failure(
                $"Account with ID '{request.Id}' was not found.");

        return Result<AccountDto>.Success(account.ToDto());
    }
}
