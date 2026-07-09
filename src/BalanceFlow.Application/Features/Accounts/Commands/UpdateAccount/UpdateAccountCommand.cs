namespace BalanceFlow.Application.Features.Accounts.Commands.UpdateAccount;

/// <summary>
/// Command to update an existing account's properties.
/// The <see cref="Id"/> identifies which account to modify.
/// </summary>
public sealed record UpdateAccountCommand(
    Guid Id,
    string AccountCode,
    string Name,
    AccountType Type,
    string? Description
) : IRequest<Result<AccountDto>>;
