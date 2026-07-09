namespace BalanceFlow.Application.Interfaces;

/// <summary>
/// Data access contract for <see cref="Account"/> entities.
/// The Infrastructure layer provides the concrete implementation using EF Core.
///
/// <para><strong>Why an interface and not a concrete class?</strong>
/// This is the Dependency Inversion Principle (the "D" in SOLID).
/// The Application layer defines <em>what</em> data operations it needs;
/// the Infrastructure layer decides <em>how</em> to implement them.
/// This means you can swap EF Core for Dapper, a REST API, or an in-memory
/// fake (for unit tests) without changing any Application code.</para>
/// </summary>
public interface IAccountRepository
{
    /// <summary>
    /// Retrieves a single account by its unique identifier.
    /// Returns <c>null</c> if no account with the given ID exists or if it is soft-deleted.
    /// </summary>
    Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paginated list of non-deleted accounts, ordered by account code.
    /// Returns the items for the requested page and the total count (for pagination metadata).
    /// </summary>
    Task<(IReadOnlyList<Account> Items, int TotalCount)> GetAllAsync(
        int pageNumber, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves an account by its unique account code.
    /// Used for uniqueness validation when creating or updating accounts.
    /// </summary>
    Task<Account?> GetByAccountCodeAsync(string accountCode, CancellationToken cancellationToken = default);

    /// <summary>Adds a new account to the data store (not persisted until <see cref="IUnitOfWork.SaveChangesAsync"/> is called).</summary>
    Task AddAsync(Account account, CancellationToken cancellationToken = default);

    /// <summary>Marks an existing account as modified (not persisted until <see cref="IUnitOfWork.SaveChangesAsync"/> is called).</summary>
    void Update(Account account);
}
