using Chickquita.Application.Interfaces;
using Chickquita.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chickquita.Infrastructure.Data;

/// <summary>
/// Application database context for Chickquita.
/// Implements multi-tenancy with Row-Level Security (RLS) support and EF Core global query filters.
/// </summary>
public class ApplicationDbContext : DbContext
{
    private readonly ICurrentUserService _currentUserService;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService currentUserService)
        : base(options)
    {
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
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

    /// <summary>
    /// Egg sales for tracking revenue.
    /// </summary>
    public DbSet<EggSale> EggSales => Set<EggSale>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from the current assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Global query filters for tenant isolation — evaluated at query time via _currentUserService.
        // These act as a defense-in-depth layer complementing PostgreSQL RLS.
        // When TenantId is null (no active request context), queries return no rows — this is intentional.
        modelBuilder.Entity<Coop>().HasQueryFilter(c => c.TenantId == _currentUserService.TenantId);
        modelBuilder.Entity<Flock>().HasQueryFilter(f => f.TenantId == _currentUserService.TenantId);
        modelBuilder.Entity<FlockHistory>().HasQueryFilter(h => h.TenantId == _currentUserService.TenantId);
        modelBuilder.Entity<DailyRecord>().HasQueryFilter(d => d.TenantId == _currentUserService.TenantId);
        modelBuilder.Entity<Purchase>().HasQueryFilter(p => p.TenantId == _currentUserService.TenantId);
        modelBuilder.Entity<EggSale>().HasQueryFilter(e => e.TenantId == _currentUserService.TenantId);
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
