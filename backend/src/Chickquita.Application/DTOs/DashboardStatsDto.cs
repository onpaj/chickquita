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
    /// Total number of roosters across all active flocks.
    /// </summary>
    public int TotalRoosters { get; set; }

    /// <summary>
    /// Total number of chicks across all active flocks.
    /// </summary>
    public int TotalChicks { get; set; }

    /// <summary>
    /// Total number of animals (hens + roosters + chicks) across all active flocks.
    /// </summary>
    public int TotalAnimals { get; set; }

    /// <summary>
    /// Total eggs recorded today across all flocks.
    /// </summary>
    public int TodayEggs { get; set; }

    /// <summary>
    /// Total eggs recorded in the last 7 days across all flocks.
    /// </summary>
    public int ThisWeekEggs { get; set; }

    /// <summary>
    /// Average eggs per day in the last 7 days.
    /// </summary>
    public decimal AvgEggsPerDay { get; set; }

    /// <summary>
    /// Current cost per egg (all-time total costs divided by all-time total eggs).
    /// Returns null when there is no production data.
    /// </summary>
    public decimal? CostPerEgg { get; set; }
}
