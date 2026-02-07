using Chickquita.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chickquita.Infrastructure.Data;

/// <summary>
/// Application database context for Chickquita.
/// Implements multi-tenancy with Row-Level Security (RLS) support.
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Tenants (user accounts) in the system.
    /// </summary>
    public DbSet<Tenant> Tenants => Set<Tenant>();

    /// <summary>
    /// Coops (chicken coop locations) in the system.
    /// </summary>
    public DbSet<Coop> Coops => Set<Coop>();

    /// <summary>
    /// Flocks (groups of chickens) in the system.
    /// </summary>
    public DbSet<Flock> Flocks => Set<Flock>();

    /// <summary>
    /// Flock history entries tracking composition changes over time.
    /// </summary>
    public DbSet<FlockHistory> FlockHistory => Set<FlockHistory>();

    /// <summary>
    /// Daily records for tracking egg production per flock.
    /// </summary>
    public DbSet<DailyRecord> DailyRecords => Set<DailyRecord>();

    /// <summary>
    /// Purchases (expenses) for chicken farming.
    /// </summary>
    public DbSet<Purchase> Purchases => Set<Purchase>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from the current assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    /// <summary>
    /// Sets the tenant context for Row-Level Security (RLS).
    /// This must be called before any queries to ensure tenant isolation.
    /// </summary>
    /// <param name="tenantId">The tenant ID to set for the current session</param>
    public async Task SetTenantContextAsync(Guid tenantId)
    {
        await Database.ExecuteSqlRawAsync(
            "SELECT set_tenant_context({0})",
            tenantId
        );
    }
}
