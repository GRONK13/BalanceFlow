namespace BalanceFlow.Application.DTOs;

/// <summary>
/// Data transfer object for a single journal entry line (posting).
/// Includes denormalized account info (code + name) so the client
/// doesn't need a separate API call to resolve account IDs.
/// </summary>
public sealed record JournalEntryLineDto(
    Guid Id,
    Guid AccountId,
    string? AccountCode,
    string? AccountName,
    decimal DebitAmount,
    decimal CreditAmount,
    string? Description
);
