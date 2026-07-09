using BalanceFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BalanceFlow.Infrastructure.Configurations;

/// <summary>
/// Entity configuration for the <see cref="User"/> entity.
/// Defines constraints, indexes, and field attributes for EF Core mappings.
/// </summary>
public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Username)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(u => u.Role)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(u => u.IsActive)
            .HasDefaultValue(true);

        builder.HasIndex(u => u.Username)
            .IsUnique();
    }
}
