namespace BalanceFlow.Application.Features.Accounts.Commands.DeleteAccount;

/// <summary>
/// Command to soft-delete an account by its ID.
/// Returns a non-generic <see cref="Result"/> (no data payload) because
/// delete operations don't need to return the deleted entity.
/// </summary>
public sealed record DeleteAccountCommand(Guid Id) : IRequest<Result>;
