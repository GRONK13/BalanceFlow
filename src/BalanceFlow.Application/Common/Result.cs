namespace BalanceFlow.Application.Common;

/// <summary>
/// A discriminated result type for operations that can succeed or fail.
/// Used by command/query handlers to return outcomes without throwing exceptions
/// for expected business-logic failures (e.g., "not found", "duplicate code").
///
/// <para><strong>Why not just throw exceptions?</strong>
/// Exceptions are for <em>unexpected</em> failures (database down, null reference).
/// Business-rule violations are <em>expected</em> outcomes — the Result pattern
/// makes that explicit in the type signature and avoids the performance cost
/// of exception stack-trace capture.</para>
/// </summary>
public class Result
{
    /// <summary>Indicates whether the operation completed successfully.</summary>
    public bool IsSuccess { get; }

    /// <summary>Indicates whether the operation failed. Inverse of <see cref="IsSuccess"/>.</summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// A list of human-readable error messages. Empty on success.
    /// Multiple errors can occur when, for example, several validation rules fail at once.
    /// </summary>
    public IReadOnlyList<string> Errors { get; }

    /// <summary>Internal constructor — use the static factory methods instead.</summary>
    protected Result(bool isSuccess, IReadOnlyList<string> errors)
    {
        IsSuccess = isSuccess;
        Errors = errors;
    }

    /// <summary>Creates a successful result with no data payload.</summary>
    public static Result Success() => new(true, []);

    /// <summary>Creates a failed result with a single error message.</summary>
    public static Result Failure(string error) => new(false, [error]);

    /// <summary>Creates a failed result with multiple error messages.</summary>
    public static Result Failure(IEnumerable<string> errors) => new(false, errors.ToList().AsReadOnly());
}

/// <summary>
/// A generic result that carries a data payload on success.
///
/// <para><strong>Usage in handlers:</strong></para>
/// <code>
/// // Success — return the data:
/// return Result&lt;AccountDto&gt;.Success(accountDto);
///
/// // Failure — return error message(s):
/// return Result&lt;AccountDto&gt;.Failure("Account not found.");
/// </code>
/// </summary>
/// <typeparam name="T">The type of data returned on success.</typeparam>
public class Result<T> : Result
{
    /// <summary>
    /// The data payload. Only meaningful when <see cref="Result.IsSuccess"/> is <c>true</c>.
    /// Will be <c>default</c> (null for reference types) on failure.
    /// </summary>
    public T? Data { get; }

    private Result(T data) : base(true, [])
    {
        Data = data;
    }

    private Result(IReadOnlyList<string> errors) : base(false, errors)
    {
        Data = default;
    }

    /// <summary>Creates a successful result carrying <paramref name="data"/>.</summary>
    public static Result<T> Success(T data) => new(data);

    /// <summary>Creates a failed result with a single error message.</summary>
    public new static Result<T> Failure(string error) => new([error]);

    /// <summary>Creates a failed result with multiple error messages.</summary>
    public new static Result<T> Failure(IEnumerable<string> errors) => new(errors.ToList().AsReadOnly());
}
