using Chickquita.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chickquita.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework Core configuration for the DailyRecord entity.
/// </summary>
public class DailyRecordConfiguration : IEntityTypeConfiguration<DailyRecord>
{
    public void Configure(EntityTypeBuilder<DailyRecord> builder)
    {
        // Table name
        builder.ToTable("daily_records");

        // Primary key
        builder.HasKey(d => d.Id);

        // Properties
        builder.Property(d => d.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(d => d.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(d => d.FlockId)
            .HasColumnName("flock_id")
            .IsRequired();

        builder.Property(d => d.RecordDate)
            .HasColumnName("record_date")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(d => d.EggCount)
            .HasColumnName("egg_count")
            .IsRequired();

        builder.Property(d => d.Notes)
            .HasColumnName("notes")
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(d => d.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(d => d.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        // Relationships
        builder.HasOne(d => d.Flock)
            .WithMany()
            .HasForeignKey(d => d.FlockId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(d => d.TenantId)
            .HasDatabaseName("ix_daily_records_tenant_id");

        builder.HasIndex(d => d.FlockId)
            .HasDatabaseName("ix_daily_records_flock_id");

        builder.HasIndex(d => d.RecordDate)
            .HasDatabaseName("ix_daily_records_record_date");

        // Unique constraint: one record per flock per date
        builder.HasIndex(d => new { d.FlockId, d.RecordDate })
            .HasDatabaseName("ix_daily_records_flock_id_record_date")
            .IsUnique();

        // Global query filter for tenant isolation
        // Note: The actual tenant isolation is enforced at the database level via RLS in PostgreSQL.
        // This filter is kept as a safety layer but commented out for now as it requires DI in OnModelCreating.
        // In production, RLS policies handle the actual filtering.
        // For proper implementation with ICurrentUserService, see: https://learn.microsoft.com/en-us/ef/core/querying/filters
        // builder.HasQueryFilter(d => d.TenantId == currentTenantId); // Would require DI in OnModelCreating

        // Temporarily disabled - rely on RLS only
        // builder.HasQueryFilter(d => EF.Property<Guid>(d, "TenantId") == Guid.Empty);
    }
}
