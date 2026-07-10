using System.Threading;
using System.Threading.Tasks;
using BalanceFlow.Application.DTOs;

namespace BalanceFlow.Application.Interfaces;

/// <summary>
/// Service contract to calculate and compile general ledger dashboard statistics.
/// </summary>
public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken);
}
