namespace BalanceFlow.Infrastructure.Data.Configurations;

/// <summary>
/// Database configuration rules for the <see cref="InvoiceLineItem"/> entity.
/// </summary>
public sealed class InvoiceLineItemConfiguration : IEntityTypeConfiguration<InvoiceLineItem>
{
    public void Configure(EntityTypeBuilder<InvoiceLineItem> builder)
    {
        builder.ToTable("InvoiceLineItems");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id)
            .ValueGeneratedNever();

        builder.Property(l => l.Description)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(l => l.Quantity)
            .IsRequired();

        builder.Property(l => l.UnitPrice)
            .IsRequired()
            .HasColumnType("numeric(18,2)");

        builder.Property(l => l.CreatedAt)
            .IsRequired();

        builder.Property(l => l.ModifiedAt);

        builder.Property(l => l.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Configure relationship with target Ledger Account.
        builder.HasOne(l => l.Account)
            .WithMany()
            .HasForeignKey(l => l.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        // Global query filter.
        builder.HasQueryFilter(l => !l.IsDeleted);
    }
}
