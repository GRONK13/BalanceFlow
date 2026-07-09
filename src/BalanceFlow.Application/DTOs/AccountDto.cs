namespace BalanceFlow.Application.DTOs;

/// <summary>
/// Data transfer object for <see cref="Account"/> entities.
/// This is what the API returns to clients — never the raw domain entity.
///
/// <para><strong>Why a <c>record</c> instead of a <c>class</c>?</strong>
/// Records are ideal for DTOs because they are immutable by default,
/// have built-in value equality (two AccountDto instances with the same
/// property values are considered equal), and auto-generate a useful
/// <c>ToString()</c>. Think of them as C#'s equivalent of Python's
/// <c>@dataclass(frozen=True)</c> or TypeScript's <c>Readonly&lt;T&gt;</c>.</para>
/// </summary>
public sealed record AccountDto(
    Guid Id,
    string AccountCode,
    string Name,
    string? Description,
    AccountType Type,
    string TypeName,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? ModifiedAt
);
