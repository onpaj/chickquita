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

    /// <summary>
    /// Gets detailed statistics for a given date range.
    /// Includes cost breakdown, production trends, cost per egg trends, and flock productivity.
    /// </summary>
    /// <param name="startDate">Start date (inclusive)</param>
    /// <param name="endDate">End date (inclusive)</param>
    /// <returns>Detailed statistics DTO</returns>
    Task<StatisticsDto> GetStatisticsAsync(DateOnly startDate, DateOnly endDate);
}
