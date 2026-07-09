using Microsoft.AspNetCore.Mvc;
using BalanceFlow.Application.Features.JournalEntries.Commands.CreateJournalEntry;
using BalanceFlow.Application.Features.JournalEntries.Commands.PostJournalEntry;
using BalanceFlow.Application.Features.JournalEntries.Commands.DeleteJournalEntry;
using BalanceFlow.Application.Features.JournalEntries.Queries.GetJournalEntryById;
using BalanceFlow.Application.Features.JournalEntries.Queries.GetAllJournalEntries;
using BalanceFlow.Application.Common;

namespace BalanceFlow.Api.Controllers;

/// <summary>
/// REST API Controller for managing ledger journal entries.
/// </summary>
[Authorize]
[ApiController]
[Route("api/journal-entries")]
public sealed class JournalEntriesController : ControllerBase
{
    private readonly ISender _sender;

    public JournalEntriesController(ISender sender)
    {
        _sender = sender;
    }

    [Authorize(Roles = "Accountant,Admin")]
    [HttpPost]
    [ProducesResponseType(typeof(JournalEntryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateJournalEntryCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(result.Errors);
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Data!.Id },
            result.Data);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(JournalEntryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetJournalEntryByIdQuery(id);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(result.Errors);
        }

        return Ok(result.Data);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<JournalEntryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllJournalEntriesQuery(pageNumber, pageSize);
        var result = await _sender.Send(query, cancellationToken);

        return Ok(result.Data);
    }

    [Authorize(Roles = "Accountant,Admin")]
    [HttpPost("{id:guid}/post")]
    [ProducesResponseType(typeof(JournalEntryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PostEntry(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new PostJournalEntryCommand(id);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase))
                ? NotFound(result.Errors)
                : BadRequest(result.Errors);
        }

        return Ok(result.Data);
    }

    [Authorize(Roles = "Accountant,Admin")]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteJournalEntryCommand(id);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase))
                ? NotFound(result.Errors)
                : BadRequest(result.Errors);
        }

        return NoContent();
    }
}
