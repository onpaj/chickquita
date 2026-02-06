using Chickquita.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chickquita.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework Core configuration for the FlockHistory entity.
/// </summary>
public class FlockHistoryConfiguration : IEntityTypeConfiguration<FlockHistory>
{
    public void Configure(EntityTypeBuilder<FlockHistory> builder)
    {
        // Table name
        builder.ToTable("flock_history");

        // Primary key
        builder.HasKey(h => h.Id);

        // Properties
        builder.Property(h => h.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(h => h.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(h => h.FlockId)
            .HasColumnName("flock_id")
            .IsRequired();

        builder.Property(h => h.ChangeDate)
            .HasColumnName("change_date")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(h => h.Hens)
            .HasColumnName("hens")
            .IsRequired();

        builder.Property(h => h.Roosters)
            .HasColumnName("roosters")
            .IsRequired();

        builder.Property(h => h.Chicks)
            .HasColumnName("chicks")
            .IsRequired();

        builder.Property(h => h.Reason)
            .HasColumnName("reason")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(h => h.Notes)
            .HasColumnName("notes")
            .HasMaxLength(500);

        builder.Property(h => h.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(h => h.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        // Relationships
        builder.HasOne(h => h.Flock)
            .WithMany(f => f.History)
            .HasForeignKey(h => h.FlockId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(h => h.TenantId)
            .HasDatabaseName("ix_flock_history_tenant_id");

        builder.HasIndex(h => h.FlockId)
            .HasDatabaseName("ix_flock_history_flock_id");

        builder.HasIndex(h => h.ChangeDate)
            .HasDatabaseName("ix_flock_history_change_date");

        // Global query filter for tenant isolation
        // Note: The actual tenant isolation is enforced at the database level via RLS in PostgreSQL.
        // This filter is kept as a safety layer but commented out for now as it requires DI in OnModelCreating.
        // In production, RLS policies handle the actual filtering.
        // For proper implementation with ICurrentUserService, see: https://learn.microsoft.com/en-us/ef/core/querying/filters
        // builder.HasQueryFilter(h => h.TenantId == currentTenantId); // Would require DI in OnModelCreating

        // Temporarily disabled - rely on RLS only
        // builder.HasQueryFilter(h => EF.Property<Guid>(h, "TenantId") == Guid.Empty);
    }
}
