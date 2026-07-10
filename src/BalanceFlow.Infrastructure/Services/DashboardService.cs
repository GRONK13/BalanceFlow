using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BalanceFlow.Application.DTOs;
using BalanceFlow.Application.Interfaces;
using BalanceFlow.Application.Mappings;
using BalanceFlow.Domain.Enums;
using BalanceFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BalanceFlow.Infrastructure.Services;

/// <summary>
/// Infrastructure service implementation that calculates real-time general ledger financial equation metrics,
/// aggregates balances, fetches chronological recent records using PgBouncer split-query optimizations,
/// and builds 6-month historical trends in-memory for performance and timezone safety.
/// </summary>
public sealed class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;

    public DashboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken)
    {
        // 1. Get counts
        int accountsCount = await _context.Accounts.CountAsync(a => !a.IsDeleted, cancellationToken);
        int journalEntriesCount = await _context.JournalEntries.CountAsync(j => !j.IsDeleted, cancellationToken);
        int invoicesCount = await _context.Invoices.CountAsync(i => !i.IsDeleted, cancellationToken);

        // 2. Fetch recent entries (using AsSplitQuery to optimize PgBouncer Transaction Mode queries)
        var recentEntriesEntities = await _context.JournalEntries
            .Include(j => j.Lines)
            .ThenInclude(l => l.Account)
            .Where(j => !j.IsDeleted)
            .OrderByDescending(j => j.TransactionDate)
            .ThenByDescending(j => j.CreatedAt)
            .Take(5)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

        var recentEntries = recentEntriesEntities
            .Select(j => j.ToDto())
            .ToList();

        // 3. Fetch recent invoices (using AsSplitQuery to optimize PgBouncer Transaction Mode queries)
        var recentInvoicesEntities = await _context.Invoices
            .Include(i => i.LineItems)
            .Where(i => !i.IsDeleted)
            .OrderByDescending(i => i.IssueDate)
            .ThenByDescending(i => i.CreatedAt)
            .Take(5)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

        var recentInvoices = recentInvoicesEntities
            .Select(i => i.ToDto())
            .ToList();

        // 4. Fetch all posted lines to calculate statistics in-memory (highly performant and timezone-safe)
        var postedLines = await _context.JournalEntryLines
            .Include(l => l.Account)
            .Include(l => l.JournalEntry)
            .Where(l => l.JournalEntry.IsPosted && !l.JournalEntry.IsDeleted && !l.Account.IsDeleted)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

        decimal assets = postedLines.Where(l => l.Account.Type == AccountType.Asset).Sum(l => l.DebitAmount - l.CreditAmount);
        decimal liabilities = postedLines.Where(l => l.Account.Type == AccountType.Liability).Sum(l => l.CreditAmount - l.DebitAmount);
        decimal equity = postedLines.Where(l => l.Account.Type == AccountType.Equity).Sum(l => l.CreditAmount - l.DebitAmount);
        decimal revenue = postedLines.Where(l => l.Account.Type == AccountType.Revenue).Sum(l => l.CreditAmount - l.DebitAmount);
        decimal expense = postedLines.Where(l => l.Account.Type == AccountType.Expense).Sum(l => l.DebitAmount - l.CreditAmount);

        decimal totalAssets = assets;
        decimal totalLiabilities = liabilities;
        decimal totalEquity = equity + (revenue - expense);

        // 5. Calculate historical trends for the last 6 months in-memory
        var assetTrend = new decimal[6];
        var equityTrend = new decimal[6];

        var today = DateTime.UtcNow;
        for (int i = 0; i < 6; i++)
        {
            var dateInMonth = today.AddMonths(i - 5);
            var endOfMonth = new DateTime(
                dateInMonth.Year, 
                dateInMonth.Month, 
                DateTime.DaysInMonth(dateInMonth.Year, dateInMonth.Month), 
                23, 59, 59, 
                DateTimeKind.Utc);

            var historicalLines = postedLines
                .Where(l => l.JournalEntry.TransactionDate <= endOfMonth)
                .ToList();

            decimal hAssets = historicalLines.Where(l => l.Account.Type == AccountType.Asset).Sum(l => l.DebitAmount - l.CreditAmount);
            decimal hEquity = historicalLines.Where(l => l.Account.Type == AccountType.Equity).Sum(l => l.CreditAmount - l.DebitAmount);
            decimal hRevenue = historicalLines.Where(l => l.Account.Type == AccountType.Revenue).Sum(l => l.CreditAmount - l.DebitAmount);
            decimal hExpense = historicalLines.Where(l => l.Account.Type == AccountType.Expense).Sum(l => l.DebitAmount - l.CreditAmount);

            assetTrend[i] = hAssets;
            equityTrend[i] = hEquity + (hRevenue - hExpense);
        }

        return new DashboardSummaryDto(
            TotalAssets: totalAssets,
            TotalLiabilities: totalLiabilities,
            TotalEquity: totalEquity,
            AccountsCount: accountsCount,
            JournalEntriesCount: journalEntriesCount,
            InvoicesCount: invoicesCount,
            RecentEntries: recentEntries,
            RecentInvoices: recentInvoices,
            AssetTrend: assetTrend,
            EquityTrend: equityTrend
        );
    }
}
