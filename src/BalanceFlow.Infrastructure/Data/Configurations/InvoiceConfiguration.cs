namespace BalanceFlow.Infrastructure.Data.Configurations;

/// <summary>
/// Database configuration rules for the <see cref="Invoice"/> entity.
/// </summary>
public sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .ValueGeneratedNever();

        builder.Property(i => i.InvoiceNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(i => i.VendorName)
            .IsRequired()
            .HasMaxLength(150);

        // Ensure unique index across InvoiceNumber + VendorName.
        builder.HasIndex(i => new { i.InvoiceNumber, i.VendorName })
            .IsUnique();

        builder.Property(i => i.IssueDate)
            .IsRequired();

        builder.Property(i => i.DueDate)
            .IsRequired();

        // Numeric precision configuration for TaxAmount and TotalAmount.
        builder.Property(i => i.TaxAmount)
            .IsRequired()
            .HasColumnType("numeric(18,2)");

        builder.Property(i => i.TotalAmount)
            .IsRequired()
            .HasColumnType("numeric(18,2)");

        builder.Property(i => i.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(i => i.AuditStatus)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(i => i.AuditNotes)
            .HasMaxLength(1000);

        builder.Property(i => i.UploadedFilePath)
            .HasMaxLength(500);

        builder.Property(i => i.ContentType)
            .HasMaxLength(100);

        builder.Property(i => i.CreatedAt)
            .IsRequired();

        builder.Property(i => i.ModifiedAt);

        builder.Property(i => i.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Configure one-to-many relationship with InvoiceLineItems.
        builder.HasMany(i => i.LineItems)
            .WithOne(l => l.Invoice)
            .HasForeignKey(l => l.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        // Global query filter.
        builder.HasQueryFilter(i => !i.IsDeleted);
    }
}
