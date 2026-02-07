using Chickquita.Application.DTOs;

namespace Chickquita.Application.Interfaces;

/// <summary>
/// Repository interface for statistics operations.
/// </summary>
public interface IStatisticsRepository
{
    /// <summary>
    /// Gets aggregated dashboard statistics for the current tenant.
    /// Optimized query that aggregates data in a single database round-trip.
    /// </summary>
    /// <returns>Dashboard statistics containing coops, flocks, and animal counts</returns>
    Task<DashboardStatsDto> GetDashboardStatsAsync();
}
