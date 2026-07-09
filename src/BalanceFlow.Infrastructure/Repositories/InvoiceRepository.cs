namespace BalanceFlow.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IInvoiceRepository"/>.
/// </summary>
public sealed class InvoiceRepository : IInvoiceRepository
{
    private readonly ApplicationDbContext _dbContext;

    public InvoiceRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Invoices
            .Include(i => i.LineItems)
                .ThenInclude(l => l.Account)
            .AsSplitQuery()
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    public async Task<Invoice?> GetByInvoiceNumberAndVendorAsync(
        string invoiceNumber,
        string vendorName,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Invoices
            .Include(i => i.LineItems)
                .ThenInclude(l => l.Account)
            .AsSplitQuery()
            .FirstOrDefaultAsync(
                i => i.InvoiceNumber.ToLower() == invoiceNumber.ToLower() &&
                     i.VendorName.ToLower() == vendorName.ToLower(),
                cancellationToken);
    }

    public async Task<(IReadOnlyList<Invoice> Items, int TotalCount)> GetAllAsync(
        int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Invoices
            .Include(i => i.LineItems)
                .ThenInclude(l => l.Account)
            .AsSplitQuery()
            .AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(i => i.IssueDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        await _dbContext.Invoices.AddAsync(invoice, cancellationToken);
    }

    public void Update(Invoice invoice)
    {
        _dbContext.Invoices.Update(invoice);
    }
}
