using Microsoft.AspNetCore.Mvc;
using BalanceFlow.Application.Features.Invoices.Commands.CreateInvoice;
using BalanceFlow.Application.Features.Invoices.Commands.AuditInvoice;
using BalanceFlow.Application.Features.Invoices.Commands.ApproveInvoice;
using BalanceFlow.Application.Features.Invoices.Commands.UploadInvoiceDocument;
using BalanceFlow.Application.Features.Invoices.Queries.GetInvoiceById;
using BalanceFlow.Application.Features.Invoices.Queries.GetAllInvoices;
using BalanceFlow.Application.Common;

namespace BalanceFlow.Api.Controllers;

/// <summary>
/// REST API Controller for managing vendor invoices and triggering audit runs.
/// </summary>
[Authorize]
[ApiController]
[Route("api/invoices")]
public sealed class InvoicesController : ControllerBase
{
    private readonly ISender _sender;

    public InvoicesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateInvoiceCommand command,
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

    [HttpPost("upload")]
    [ProducesResponseType(typeof(OcrExtractionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Upload(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest("No file was uploaded.");
        }

        // 1. Validation: enforce limits (max 10MB)
        if (file.Length > 10 * 1024 * 1024)
        {
            return BadRequest("File size exceeds the 10MB limit.");
        }

        // 2. Validation: enforce content formats
        var allowedContentTypes = new[] { "application/pdf", "image/png", "image/jpeg", "image/jpg" };
        if (!allowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
        {
            return BadRequest("Invalid file type. Only PDF, PNG, and JPEG formats are supported.");
        }

        // 3. Dispatch stream extraction
        using (var stream = file.OpenReadStream())
        {
            var command = new UploadInvoiceDocumentCommand(stream, file.FileName, file.ContentType);
            var result = await _sender.Send(command, cancellationToken);

            if (result.IsFailure)
            {
                return BadRequest(result.Errors);
            }

            return Ok(result.Data);
        }
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetInvoiceByIdQuery(id);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(result.Errors);
        }

        return Ok(result.Data);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<InvoiceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllInvoicesQuery(pageNumber, pageSize);
        var result = await _sender.Send(query, cancellationToken);

        return Ok(result.Data);
    }

    [Authorize(Roles = "Auditor,Admin")]
    [HttpPost("{id:guid}/audit")]
    [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Audit(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new AuditInvoiceCommand(id);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase))
                ? NotFound(result.Errors)
                : BadRequest(result.Errors);
        }

        return Ok(result.Data);
    }

    [Authorize(Roles = "Auditor,Admin")]
    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Approve(
        Guid id,
        [FromBody] ApproveInvoiceCommandBody body,
        CancellationToken cancellationToken)
    {
        var command = new ApproveInvoiceCommand(id, body.AccountsPayableAccountId);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase))
                ? NotFound(result.Errors)
                : BadRequest(result.Errors);
        }

        return Ok(result.Data);
    }
}

/// <summary>
/// HTTP Body parameter wrapper for ApproveInvoice route.
/// </summary>
public sealed record ApproveInvoiceCommandBody(Guid AccountsPayableAccountId);
