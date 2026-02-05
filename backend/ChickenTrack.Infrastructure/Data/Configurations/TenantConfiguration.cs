using ChickenTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChickenTrack.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework Core configuration for the Tenant entity.
/// </summary>
public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        // Table name
        builder.ToTable("tenants");

        // Primary key
        builder.HasKey(t => t.Id);

        // Properties
        builder.Property(t => t.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(t => t.ClerkUserId)
            .HasColumnName("clerk_user_id")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(t => t.Email)
            .HasColumnName("email")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        // Indexes
        builder.HasIndex(t => t.ClerkUserId)
            .HasDatabaseName("ix_tenants_clerk_user_id")
            .IsUnique();

        builder.HasIndex(t => t.Email)
            .HasDatabaseName("ix_tenants_email");
    }
}
