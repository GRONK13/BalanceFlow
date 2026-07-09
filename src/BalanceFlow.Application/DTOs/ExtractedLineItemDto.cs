namespace BalanceFlow.Application.DTOs;

/// <summary>
/// DTO representing itemized lines extracted from document scan.
/// </summary>
public sealed record ExtractedLineItemDto(
    string Description,
    int Quantity,
    decimal UnitPrice,
    Guid? SuggestedAccountId
);
