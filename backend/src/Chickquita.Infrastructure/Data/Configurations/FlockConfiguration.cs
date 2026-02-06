using Chickquita.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chickquita.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework Core configuration for the Flock entity.
/// </summary>
public class FlockConfiguration : IEntityTypeConfiguration<Flock>
{
    public void Configure(EntityTypeBuilder<Flock> builder)
    {
        // Table name
        builder.ToTable("flocks");

        // Primary key
        builder.HasKey(f => f.Id);

        // Properties
        builder.Property(f => f.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(f => f.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(f => f.CoopId)
            .HasColumnName("coop_id")
            .IsRequired();

        builder.Property(f => f.Identifier)
            .HasColumnName("identifier")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(f => f.HatchDate)
            .HasColumnName("hatch_date")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(f => f.CurrentHens)
            .HasColumnName("current_hens")
            .IsRequired();

        builder.Property(f => f.CurrentRoosters)
            .HasColumnName("current_roosters")
            .IsRequired();

        builder.Property(f => f.CurrentChicks)
            .HasColumnName("current_chicks")
            .IsRequired();

        builder.Property(f => f.IsActive)
            .HasColumnName("is_active")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(f => f.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(f => f.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        // Relationships
        builder.HasOne(f => f.Coop)
            .WithMany()
            .HasForeignKey(f => f.CoopId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(f => f.History)
            .WithOne(h => h.Flock)
            .HasForeignKey(h => h.FlockId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(f => f.TenantId)
            .HasDatabaseName("ix_flocks_tenant_id");

        builder.HasIndex(f => f.CoopId)
            .HasDatabaseName("ix_flocks_coop_id");

        builder.HasIndex(f => new { f.CoopId, f.Identifier })
            .HasDatabaseName("ix_flocks_coop_id_identifier")
            .IsUnique();

        builder.HasIndex(f => f.IsActive)
            .HasDatabaseName("ix_flocks_is_active");

        builder.HasIndex(f => f.HatchDate)
            .HasDatabaseName("ix_flocks_hatch_date");

        // Global query filter for tenant isolation
        // Note: The actual tenant isolation is enforced at the database level via RLS in PostgreSQL.
        // This filter is kept as a safety layer but commented out for now as it requires DI in OnModelCreating.
        // In production, RLS policies handle the actual filtering.
        // For proper implementation with ICurrentUserService, see: https://learn.microsoft.com/en-us/ef/core/querying/filters
        // builder.HasQueryFilter(f => f.TenantId == currentTenantId); // Would require DI in OnModelCreating

        // Temporarily disabled - rely on RLS only
        // builder.HasQueryFilter(f => EF.Property<Guid>(f, "TenantId") == Guid.Empty);
    }
}
