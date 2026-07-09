namespace BalanceFlow.Application.Features.Accounts.Queries.GetAllAccounts;

/// <summary>
/// Handles <see cref="GetAllAccountsQuery"/> — retrieves a paginated list of accounts,
/// maps each to a DTO, and wraps in a <see cref="PagedResult{T}"/>.
/// </summary>
public sealed class GetAllAccountsQueryHandler
    : IRequestHandler<GetAllAccountsQuery, Result<PagedResult<AccountDto>>>
{
    private readonly IAccountRepository _accountRepository;

    public GetAllAccountsQueryHandler(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task<Result<PagedResult<AccountDto>>> Handle(
        GetAllAccountsQuery request,
        CancellationToken cancellationToken)
    {
        // The repository returns a tuple: (items for the requested page, total count).
        var (items, totalCount) = await _accountRepository.GetAllAsync(
            request.PageNumber, request.PageSize, cancellationToken);

        // Map domain entities → DTOs.
        var dtos = items.Select(a => a.ToDto()).ToList();

        // Wrap in PagedResult which computes TotalPages, HasNext/PreviousPage.
        var pagedResult = new PagedResult<AccountDto>(
            dtos, totalCount, request.PageNumber, request.PageSize);

        return Result<PagedResult<AccountDto>>.Success(pagedResult);
    }
}
