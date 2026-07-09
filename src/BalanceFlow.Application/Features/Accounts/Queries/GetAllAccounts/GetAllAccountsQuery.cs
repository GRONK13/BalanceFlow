namespace BalanceFlow.Application.Features.Accounts.Queries.GetAllAccounts;

/// <summary>
/// Query to retrieve a paginated list of all active accounts.
///
/// <para><strong>C# Concept — Default Parameter Values:</strong>
/// <c>int PageNumber = 1</c> means callers can omit this parameter and it
/// defaults to 1. This is equivalent to <c>def get_all(page_number=1)</c>
/// in Python or <c>function getAll(pageNumber = 1)</c> in JavaScript.</para>
/// </summary>
public sealed record GetAllAccountsQuery(
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<Result<PagedResult<AccountDto>>>;
