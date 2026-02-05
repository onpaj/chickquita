using ChickenTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChickenTrack.Infrastructure.Data;

/// <summary>
/// Application database context for ChickenTrack.
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
