namespace BalanceFlow.Application.DTOs;

/// <summary>
/// DTO representing an Invoice Line Item.
/// </summary>
public sealed record InvoiceLineItemDto(
    Guid Id,
    Guid AccountId,
    string? AccountCode,
    string? AccountName,
    string Description,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal
);
