using System.Collections.Generic;

namespace BalanceFlow.Application.DTOs;

/// <summary>
/// Data transfer object containing real-time calculated general ledger statistics, counts, recent logs,
/// and monthly balances for trending graphs.
/// </summary>
public sealed record DashboardSummaryDto(
    decimal TotalAssets,
    decimal TotalLiabilities,
    decimal TotalEquity,
    int AccountsCount,
    int JournalEntriesCount,
    int InvoicesCount,
    IReadOnlyList<JournalEntryDto> RecentEntries,
    IReadOnlyList<InvoiceDto> RecentInvoices,
    decimal[] AssetTrend,
    decimal[] EquityTrend
);
