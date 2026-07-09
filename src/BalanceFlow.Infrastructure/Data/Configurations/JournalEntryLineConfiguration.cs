namespace BalanceFlow.Infrastructure.Data.Configurations;

/// <summary>
/// Database configuration rules for the <see cref="JournalEntryLine"/> entity.
/// </summary>
public sealed class JournalEntryLineConfiguration : IEntityTypeConfiguration<JournalEntryLine>
{
    public void Configure(EntityTypeBuilder<JournalEntryLine> builder)
    {
        builder.ToTable("JournalEntryLines");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id)
            .ValueGeneratedNever();

        // Map decimal amounts explicitly to the numeric type for precise financial math.
        builder.Property(l => l.DebitAmount)
            .IsRequired()
            .HasColumnType("numeric(18,2)");

        builder.Property(l => l.CreditAmount)
            .IsRequired()
            .HasColumnType("numeric(18,2)");

        builder.Property(l => l.Description)
            .HasMaxLength(500);

        builder.Property(l => l.CreatedAt)
            .IsRequired();

        builder.Property(l => l.ModifiedAt);

        builder.Property(l => l.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Configure the relationship with the Account.
        builder.HasOne(l => l.Account)
            .WithMany(a => a.JournalEntryLines)
            .HasForeignKey(l => l.AccountId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent account deletion if it has posted entries.

        // Global query filter.
        builder.HasQueryFilter(l => !l.IsDeleted);
    }
}
