namespace BalanceFlow.Infrastructure.Data;

/// <summary>
/// EF Core database context for the BalanceFlow application.
/// Coordinates database operations, tracks entity state, and implements the Application layer's <see cref="IUnitOfWork"/> contract.
/// </summary>
public sealed class ApplicationDbContext : DbContext, IUnitOfWork
{
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<JournalEntryLine> JournalEntryLines => Set<JournalEntryLine>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceLineItem> InvoiceLineItems => Set<InvoiceLineItem>();

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Automatically discovers and applies all IEntityTypeConfiguration classes in the current assembly.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// Persists all tracked changes to the PostgreSQL database.
    /// Implements the IUnitOfWork interface.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }
}
