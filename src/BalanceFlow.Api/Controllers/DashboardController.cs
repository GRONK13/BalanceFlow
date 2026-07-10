using Microsoft.AspNetCore.Mvc;
using MediatR;
using BalanceFlow.Application.Features.Dashboard.Queries.GetDashboardSummary;
using BalanceFlow.Application.Common;
using Microsoft.AspNetCore.Authorization;
using BalanceFlow.Application.DTOs;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace BalanceFlow.Api.Controllers;

/// <summary>
/// REST API Controller for retrieving dynamic general ledger dashboard metrics and reports.
/// </summary>
[Authorize]
[ApiController]
[Route("api/dashboard")]
public sealed class DashboardController : ControllerBase
{
    private readonly ISender _sender;

    public DashboardController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Computes and retrieves real-time financial metrics, item counts, trend charts, and audit alerts.
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(DashboardSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetDashboardSummaryQuery(), cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(result.Errors);
        }

        return Ok(result.Data);
    }
}
