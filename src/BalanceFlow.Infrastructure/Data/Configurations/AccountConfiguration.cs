namespace BalanceFlow.Infrastructure.Data.Configurations;

/// <summary>
/// Database configuration rules for the <see cref="Account"/> entity.
/// Uses the Fluent API to map properties to PostgreSQL columns.
/// </summary>
public sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Accounts");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .ValueGeneratedNever();

        builder.Property(a => a.AccountCode)
            .IsRequired()
            .HasMaxLength(20);

        // Enforce uniqueness on AccountCode at the database level.
        builder.HasIndex(a => a.AccountCode)
            .IsUnique();

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(a => a.Description)
            .HasMaxLength(500);

        // Convert the AccountType enum to its integer representation in the database.
        builder.Property(a => a.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(a => a.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(a => a.CreatedAt)
            .IsRequired();

        builder.Property(a => a.ModifiedAt);

        builder.Property(a => a.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Global query filter — automatically excludes soft-deleted records from all queries.
        builder.HasQueryFilter(a => !a.IsDeleted);
    }
}
