using Chickquita.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chickquita.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework Core configuration for the Coop entity.
/// </summary>
public class CoopConfiguration : IEntityTypeConfiguration<Coop>
{
    public void Configure(EntityTypeBuilder<Coop> builder)
    {
        // Table name
        builder.ToTable("coops");

        // Primary key
        builder.HasKey(c => c.Id);

        // Properties
        builder.Property(c => c.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(c => c.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(c => c.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.Location)
            .HasColumnName("location")
            .HasMaxLength(200);

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        // Relationships
        builder.HasOne(c => c.Tenant)
            .WithMany()
            .HasForeignKey(c => c.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(c => c.TenantId)
            .HasDatabaseName("ix_coops_tenant_id");

        // Global query filter for tenant isolation
        builder.HasQueryFilter(c => EF.Property<Guid>(c, "TenantId") == Guid.Empty);
    }
}
