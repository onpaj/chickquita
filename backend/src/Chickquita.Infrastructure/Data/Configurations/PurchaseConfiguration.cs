using Chickquita.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chickquita.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework Core configuration for the Purchase entity.
/// </summary>
public class PurchaseConfiguration : IEntityTypeConfiguration<Purchase>
{
    public void Configure(EntityTypeBuilder<Purchase> builder)
    {
        // Table name
        builder.ToTable("purchases");

        // Primary key
        builder.HasKey(p => p.Id);

        // Properties
        builder.Property(p => p.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(p => p.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(p => p.CoopId)
            .HasColumnName("coop_id")
            .IsRequired(false);

        builder.Property(p => p.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.Amount)
            .HasColumnName("amount")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(p => p.Quantity)
            .HasColumnName("quantity")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(p => p.Unit)
            .HasColumnName("unit")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.PurchaseDate)
            .HasColumnName("purchase_date")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(p => p.ConsumedDate)
            .HasColumnName("consumed_date")
            .HasColumnType("timestamp with time zone")
            .IsRequired(false);

        builder.Property(p => p.Notes)
            .HasColumnName("notes")
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        // Relationships
        builder.HasOne(p => p.Tenant)
            .WithMany()
            .HasForeignKey(p => p.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Coop)
            .WithMany()
            .HasForeignKey(p => p.CoopId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        // Indexes
        builder.HasIndex(p => p.TenantId)
            .HasDatabaseName("ix_purchases_tenant_id");

        builder.HasIndex(p => p.CoopId)
            .HasDatabaseName("ix_purchases_coop_id");

        builder.HasIndex(p => p.PurchaseDate)
            .HasDatabaseName("ix_purchases_purchase_date");

        builder.HasIndex(p => p.Type)
            .HasDatabaseName("ix_purchases_type");

        // Composite index for common queries (tenant + purchase date)
        builder.HasIndex(p => new { p.TenantId, p.PurchaseDate })
            .HasDatabaseName("ix_purchases_tenant_id_purchase_date");

        // Global query filter for tenant isolation
        // Note: The actual tenant isolation is enforced at the database level via RLS in PostgreSQL.
        // This filter is kept as a safety layer but commented out for now as it requires DI in OnModelCreating.
        // In production, RLS policies handle the actual filtering.
        // For proper implementation with ICurrentUserService, see: https://learn.microsoft.com/en-us/ef/core/querying/filters
        // builder.HasQueryFilter(p => p.TenantId == currentTenantId); // Would require DI in OnModelCreating

        // Temporarily disabled - rely on RLS only
        // builder.HasQueryFilter(p => EF.Property<Guid>(p, "TenantId") == Guid.Empty);
    }
}
