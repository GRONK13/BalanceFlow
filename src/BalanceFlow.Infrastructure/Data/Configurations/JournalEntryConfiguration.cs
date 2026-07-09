namespace BalanceFlow.Infrastructure.Data.Configurations;

/// <summary>
/// Database configuration rules for the <see cref="JournalEntry"/> aggregate root.
/// </summary>
public sealed class JournalEntryConfiguration : IEntityTypeConfiguration<JournalEntry>
{
    public void Configure(EntityTypeBuilder<JournalEntry> builder)
    {
        builder.ToTable("JournalEntries");

        builder.HasKey(j => j.Id);

        builder.Property(j => j.Id)
            .ValueGeneratedNever();

        builder.Property(j => j.ReferenceNumber)
            .IsRequired()
            .HasMaxLength(50);

        // Reference number must be unique.
        builder.HasIndex(j => j.ReferenceNumber)
            .IsUnique();

        builder.Property(j => j.TransactionDate)
            .IsRequired();

        builder.Property(j => j.Description)
            .HasMaxLength(500);

        builder.Property(j => j.IsPosted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(j => j.PostedAt);

        builder.Property(j => j.CreatedAt)
            .IsRequired();

        builder.Property(j => j.ModifiedAt);

        builder.Property(j => j.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Configure the one-to-many relationship with JournalEntryLines.
        // We configure the relationship using backing fields since the private _lines collection is encapsulated.
        builder.HasMany(j => j.Lines)
            .WithOne(l => l.JournalEntry)
            .HasForeignKey(l => l.JournalEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        // Global query filter — automatically excludes soft-deleted records.
        builder.HasQueryFilter(j => !j.IsDeleted);
    }
}
