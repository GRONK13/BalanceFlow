namespace BalanceFlow.Application.Features.Accounts.Commands.CreateAccount;

/// <summary>
/// Command to create a new account in the chart of accounts.
///
/// <para><strong>C# Concept — <c>record</c> as a command:</strong>
/// Commands are just data containers — they carry the "what" (the data)
/// but not the "how" (the logic). Using a <c>record</c> makes them
/// immutable and gives free value-equality, <c>ToString()</c>, and
/// <c>with</c>-expression support.</para>
///
/// <para><strong>C# Concept — <c>IRequest&lt;T&gt;</c> (MediatR):</strong>
/// By implementing <c>IRequest&lt;Result&lt;AccountDto&gt;&gt;</c>, this record
/// tells MediatR: "I'm a message, and whoever handles me must return a
/// <c>Result&lt;AccountDto&gt;</c>." MediatR automatically finds the matching
/// <c>IRequestHandler&lt;CreateAccountCommand, Result&lt;AccountDto&gt;&gt;</c>
/// implementation and routes the message to it.</para>
/// </summary>
public sealed record CreateAccountCommand(
    string AccountCode,
    string Name,
    AccountType Type,
    string? Description
) : IRequest<Result<AccountDto>>;
