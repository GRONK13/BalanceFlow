namespace BalanceFlow.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IAccountRepository"/>.
/// Handles data operations for the <see cref="Account"/> entity.
/// </summary>
public sealed class AccountRepository : IAccountRepository
{
    private readonly ApplicationDbContext _dbContext;

    public AccountRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Accounts
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<(IReadOnlyList<Account> Items, int TotalCount)> GetAllAsync(
        int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Accounts.AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(a => a.AccountCode)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<Account?> GetByAccountCodeAsync(string accountCode, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Accounts
            .FirstOrDefaultAsync(a => a.AccountCode == accountCode, cancellationToken);
    }

    public async Task AddAsync(Account account, CancellationToken cancellationToken = default)
    {
        await _dbContext.Accounts.AddAsync(account, cancellationToken);
    }

    public void Update(Account account)
    {
        _dbContext.Accounts.Update(account);
    }
}
