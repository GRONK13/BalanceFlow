using MediatR;
using BalanceFlow.Application.Common;
using BalanceFlow.Application.DTOs;

namespace BalanceFlow.Application.Features.Dashboard.Queries.GetDashboardSummary;

/// <summary>
/// CQRS query request to fetch the real-time compiled accounting dashboard statistics.
/// </summary>
public sealed record GetDashboardSummaryQuery() : IRequest<Result<DashboardSummaryDto>>;
