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
            "SELECT set_config('app.current_tenant_id', {0}, false)",
            tenantId.ToString()
        );
    }
}
