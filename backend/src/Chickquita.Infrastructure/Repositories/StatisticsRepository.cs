using Chickquita.Application.DTOs;
using Chickquita.Application.Interfaces;
using Chickquita.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Chickquita.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for statistics operations.
/// Provides optimized aggregation queries for dashboard statistics.
/// </summary>
public class StatisticsRepository : IStatisticsRepository
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="StatisticsRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public StatisticsRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Gets aggregated dashboard statistics for the current tenant.
    /// Uses two optimized queries: one for flock aggregation, one for coop count.
    /// Tenant isolation is enforced by Row-Level Security (RLS) at the database level.
    /// </summary>
    /// <returns>Dashboard statistics containing coops, flocks, and animal counts</returns>
    public async Task<DashboardStatsDto> GetDashboardStatsAsync()
    {
        // Optimized single-query aggregation for flocks
        // Tenant isolation is handled by RLS at database level
        var flockStats = await _context.Flocks
            .Where(f => f.IsActive)
            .GroupBy(f => 1) // Group all records together for aggregation
            .Select(g => new
            {
                ActiveFlocks = g.Count(),
                TotalHens = g.Sum(f => f.CurrentHens),
                TotalAnimals = g.Sum(f => f.CurrentHens + f.CurrentRoosters + f.CurrentChicks)
            })
            .FirstOrDefaultAsync();

        // Get count of active coops (separate query but optimized with counting)
        var totalCoops = await _context.Coops
            .Where(c => c.IsActive)
            .CountAsync();

        // If no flocks exist, return zero stats
        return new DashboardStatsDto
        {
            TotalCoops = totalCoops,
            ActiveFlocks = flockStats?.ActiveFlocks ?? 0,
            TotalHens = flockStats?.TotalHens ?? 0,
            TotalAnimals = flockStats?.TotalAnimals ?? 0
        };
    }
}
