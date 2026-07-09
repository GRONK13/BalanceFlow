namespace BalanceFlow.Domain.Exceptions;

/// <summary>
/// Base exception for all domain-level invariant violations.
/// Provides a common catch target for the Application layer's
/// exception-handling pipeline or MediatR behavior.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }

    protected DomainException(string message, Exception innerException)
        : base(message, innerException) { }
}
