using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BalanceFlow.Application.Exceptions;
using BalanceFlow.Domain.Exceptions;

namespace BalanceFlow.Api.Middleware;

/// <summary>
/// Global exception handler that catches all unhandled exceptions in the request pipeline.
/// Converts exceptions into standardized RFC 7807 Problem Details.
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

        var problemDetails = CreateProblemDetails(exception, httpContext);

        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static ProblemDetails CreateProblemDetails(Exception exception, HttpContext httpContext)
    {
        return exception switch
        {
            // FluentValidation errors caught by our ValidationBehavior pipeline
            ValidationException validationException => new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Validation failure",
                Detail = "One or more validation errors occurred.",
                Instance = httpContext.Request.Path,
                Extensions = { ["errors"] = validationException.Errors }
            },

            // Double-entry balancing violations or business rules from Domain Layer
            DomainException domainException => new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Domain business rule violation",
                Detail = domainException.Message,
                Instance = httpContext.Request.Path
            },

            // General fallthrough exception handling
            _ => new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred. Please contact system administrator.",
                Instance = httpContext.Request.Path
            }
        };
    }
}
