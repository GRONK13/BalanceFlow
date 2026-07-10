using System.Threading;
using System.Threading.Tasks;
using MediatR;
using BalanceFlow.Application.Common;
using BalanceFlow.Application.DTOs;
using BalanceFlow.Application.Interfaces;

namespace BalanceFlow.Application.Features.Dashboard.Queries.GetDashboardSummary;

/// <summary>
/// CQRS handler that routes the dashboard query request to the injected DashboardService.
/// </summary>
public sealed class GetDashboardSummaryQueryHandler
    : IRequestHandler<GetDashboardSummaryQuery, Result<DashboardSummaryDto>>
{
    private readonly IDashboardService _dashboardService;

    public GetDashboardSummaryQueryHandler(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    public async Task<Result<DashboardSummaryDto>> Handle(
        GetDashboardSummaryQuery request, 
        CancellationToken cancellationToken)
    {
        try
        {
            var summary = await _dashboardService.GetSummaryAsync(cancellationToken);
            return Result<DashboardSummaryDto>.Success(summary);
        }
        catch (System.Exception ex)
        {
            return Result<DashboardSummaryDto>.Failure(ex.Message);
        }
    }
}
