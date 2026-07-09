namespace BalanceFlow.Application.Exceptions;

/// <summary>
/// Thrown by <see cref="Behaviors.ValidationBehavior{TRequest, TResponse}"/> when
/// one or more FluentValidation rules fail before the handler executes.
///
/// <para>The API layer's exception-handling middleware should catch this and return
/// an HTTP 400 Bad Request with the error list in the response body.</para>
/// </summary>
public sealed class ValidationException : Exception
{
    /// <summary>The list of human-readable validation error messages.</summary>
    public IReadOnlyList<string> Errors { get; }

    public ValidationException(IReadOnlyList<string> errors)
        : base("One or more validation failures have occurred.")
    {
        Errors = errors;
    }
}
