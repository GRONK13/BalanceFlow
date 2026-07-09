using BalanceFlow.Application.Exceptions;

namespace BalanceFlow.Application.Behaviors;

/// <summary>
/// A MediatR pipeline behavior that automatically validates every incoming
/// request before the handler executes. This is the CQRS equivalent of
/// Express.js middleware or Django middleware.
///
/// <para><strong>How it works:</strong></para>
/// <list type="number">
///   <item>MediatR sends a request (e.g., <c>CreateAccountCommand</c>) through the pipeline.</item>
///   <item>This behavior discovers all <see cref="IValidator{T}"/> registered for that request type.</item>
///   <item>It runs every validator in parallel and collects the errors.</item>
///   <item>If any errors exist → throws <see cref="ValidationException"/> (handler never runs).</item>
///   <item>If no errors → calls <c>next()</c> to pass control to the handler.</item>
/// </list>
///
/// <para><strong>C# Concept — <c>IPipelineBehavior&lt;TRequest, TResponse&gt;</c>:</strong>
/// Think of this as middleware for MediatR. The <c>next</c> delegate is equivalent
/// to calling <c>next()</c> in Express — it passes control to the next behavior
/// in the pipeline, or to the final handler if this is the last behavior.</para>
/// </summary>
/// <typeparam name="TRequest">The command or query type (e.g., <c>CreateAccountCommand</c>).</typeparam>
/// <typeparam name="TResponse">The response type (e.g., <c>Result&lt;AccountDto&gt;</c>).</typeparam>
public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    // MediatR's DI integration injects ALL validators registered for TRequest.
    // If no validator exists for a given request, this collection is simply empty.
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // If there are no validators for this request type, skip straight to the handler.
        if (!_validators.Any())
            return await next();

        // Create a validation context for the incoming request.
        var context = new ValidationContext<TRequest>(request);

        // Run ALL validators in parallel for maximum performance.
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        // Collect all error messages, removing duplicates.
        var errors = validationResults
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .Select(failure => failure.ErrorMessage)
            .Distinct()
            .ToList();

        // If any validation errors exist, throw before the handler runs.
        if (errors.Count > 0)
            throw new Application.Exceptions.ValidationException(errors);

        // All validation passed — forward to the next behavior or handler.
        return await next();
    }
}
