using Chickquita.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chickquita.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework Core configuration for the EggSale entity.
/// </summary>
public class EggSaleConfiguration : IEntityTypeConfiguration<EggSale>
{
    public void Configure(EntityTypeBuilder<EggSale> builder)
    {
        // Table name
        builder.ToTable("egg_sales");

        // Primary key
        builder.HasKey(e => e.Id);

        // Properties
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(e => e.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(e => e.Date)
            .HasColumnName("date")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(e => e.Quantity)
            .HasColumnName("quantity")
            .IsRequired();

        builder.Property(e => e.PricePerUnit)
            .HasColumnName("price_per_unit")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(e => e.BuyerName)
            .HasColumnName("buyer_name")
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(e => e.Notes)
            .HasColumnName("notes")
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        // Relationships
        builder.HasOne(e => e.Tenant)
            .WithMany()
            .HasForeignKey(e => e.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(e => e.TenantId)
            .HasDatabaseName("ix_egg_sales_tenant_id");

        builder.HasIndex(e => e.Date)
            .HasDatabaseName("ix_egg_sales_date");

        // Composite index for common queries (tenant + date)
        builder.HasIndex(e => new { e.TenantId, e.Date })
            .HasDatabaseName("ix_egg_sales_tenant_id_date");
    }
}
