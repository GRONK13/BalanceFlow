namespace BalanceFlow.Application.Features.Accounts.Queries.GetAccountById;

/// <summary>
/// Query to retrieve a single account by its unique identifier.
///
/// <para><strong>CQRS Naming Convention:</strong>
/// Commands are verbs ("Create", "Update", "Delete") — they change state.
/// Queries are questions ("Get", "List", "Search") — they read state.
/// This naming makes intent immediately clear from the class name alone.</para>
/// </summary>
public sealed record GetAccountByIdQuery(Guid Id) : IRequest<Result<AccountDto>>;
