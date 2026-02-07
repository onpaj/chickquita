namespace Chickquita.Application.DTOs;

/// <summary>
/// Data Transfer Object for dashboard statistics.
/// </summary>
public sealed class DashboardStatsDto
{
    /// <summary>
    /// Total number of active coops for the current tenant.
    /// </summary>
    public int TotalCoops { get; set; }

    /// <summary>
    /// Total number of active flocks across all coops for the current tenant.
    /// </summary>
    public int ActiveFlocks { get; set; }

    /// <summary>
    /// Total number of hens across all active flocks.
    /// </summary>
    public int TotalHens { get; set; }

    /// <summary>
    /// Total number of animals (hens + roosters + chicks) across all active flocks.
    /// </summary>
    public int TotalAnimals { get; set; }
}
