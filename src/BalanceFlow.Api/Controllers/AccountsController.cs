using Microsoft.AspNetCore.Mvc;
using BalanceFlow.Application.Features.Accounts.Commands.CreateAccount;
using BalanceFlow.Application.Features.Accounts.Commands.UpdateAccount;
using BalanceFlow.Application.Features.Accounts.Commands.DeleteAccount;
using BalanceFlow.Application.Features.Accounts.Queries.GetAccountById;
using BalanceFlow.Application.Features.Accounts.Queries.GetAllAccounts;
using BalanceFlow.Application.Common;

namespace BalanceFlow.Api.Controllers;

/// <summary>
/// REST API Controller for managing ledger accounts.
/// Coordinates HTTP routes with corresponding MediatR commands and queries.
/// </summary>
[Authorize]
[ApiController]
[Route("api/accounts")]
public sealed class AccountsController : ControllerBase
{
    private readonly ISender _sender;

    public AccountsController(ISender sender)
    {
        _sender = sender;
    }

    [Authorize(Roles = "Accountant,Admin")]
    [HttpPost]
    [ProducesResponseType(typeof(AccountDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateAccountCommand command,
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
    [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetAccountByIdQuery(id);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(result.Errors);
        }

        return Ok(result.Data);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AccountDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllAccountsQuery(pageNumber, pageSize);
        var result = await _sender.Send(query, cancellationToken);

        return Ok(result.Data);
    }

    [Authorize(Roles = "Accountant,Admin")]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateAccountCommand command,
        CancellationToken cancellationToken)
    {
        // Enforce path ID matches body ID.
        if (id != command.Id)
        {
            return BadRequest("Path ID does not match request body ID.");
        }

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            // If error indicates not found, return NotFound. Otherwise return BadRequest.
            return result.Errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase))
                ? NotFound(result.Errors)
                : BadRequest(result.Errors);
        }

        return Ok(result.Data);
    }

    [Authorize(Roles = "Accountant,Admin")]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteAccountCommand(id);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(result.Errors);
        }

        return NoContent();
    }
}
