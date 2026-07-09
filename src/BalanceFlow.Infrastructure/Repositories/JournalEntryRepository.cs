namespace BalanceFlow.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IJournalEntryRepository"/>.
/// Handles eager loading of lines and account details.
/// </summary>
public sealed class JournalEntryRepository : IJournalEntryRepository
{
    private readonly ApplicationDbContext _dbContext;

    public JournalEntryRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<JournalEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.JournalEntries
            .Include(j => j.Lines)
                .ThenInclude(l => l.Account)
            .AsSplitQuery()
            .FirstOrDefaultAsync(j => j.Id == id, cancellationToken);
    }

    public async Task<(IReadOnlyList<JournalEntry> Items, int TotalCount)> GetAllAsync(
        int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.JournalEntries
            .Include(j => j.Lines)
                .ThenInclude(l => l.Account)
            .AsSplitQuery()
            .AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(j => j.TransactionDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<JournalEntry?> GetByReferenceNumberAsync(string referenceNumber, CancellationToken cancellationToken = default)
    {
        return await _dbContext.JournalEntries
            .Include(j => j.Lines)
                .ThenInclude(l => l.Account)
            .AsSplitQuery()
            .FirstOrDefaultAsync(j => j.ReferenceNumber == referenceNumber, cancellationToken);
    }

    public async Task AddAsync(JournalEntry journalEntry, CancellationToken cancellationToken = default)
    {
        await _dbContext.JournalEntries.AddAsync(journalEntry, cancellationToken);
    }

    public void Update(JournalEntry journalEntry)
    {
        _dbContext.JournalEntries.Update(journalEntry);
    }
}
