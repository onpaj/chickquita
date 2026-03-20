namespace Chickquita.Application.DTOs;

/// <summary>
/// Data Transfer Object for detailed statistics and analytics.
/// </summary>
public sealed class StatisticsDto
{
    /// <summary>
    /// Cost breakdown by purchase type.
    /// </summary>
    public List<CostBreakdownItemDto> CostBreakdown { get; set; } = new();

    /// <summary>
    /// Production trend over time (daily).
    /// </summary>
    public List<ProductionTrendItemDto> ProductionTrend { get; set; } = new();

    /// <summary>
    /// Cost per egg trend over time (daily).
    /// </summary>
    public List<CostPerEggTrendItemDto> CostPerEggTrend { get; set; } = new();

    /// <summary>
    /// Flock productivity comparison.
    /// </summary>
    public List<FlockProductivityItemDto> FlockProductivity { get; set; } = new();

    /// <summary>
    /// Monthly revenue vs. costs trend.
    /// </summary>
    public List<RevenueTrendItemDto> RevenueTrend { get; set; } = new();

    /// <summary>
    /// Summary statistics for the period.
    /// </summary>
    public StatisticsSummaryDto Summary { get; set; } = new();
}

/// <summary>
/// Cost breakdown by purchase type.
/// </summary>
public sealed class CostBreakdownItemDto
{
    /// <summary>
    /// Purchase type (e.g., "feed", "bedding", "veterinary").
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Total amount spent on this type.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Percentage of total costs.
    /// </summary>
    public decimal Percentage { get; set; }
}

/// <summary>
/// Production trend data point.
/// </summary>
public sealed class ProductionTrendItemDto
{
    /// <summary>
    /// Date of the data point (YYYY-MM-DD).
    /// </summary>
    public string Date { get; set; } = string.Empty;

    /// <summary>
    /// Number of eggs produced on this date.
    /// </summary>
    public int Eggs { get; set; }
}

/// <summary>
/// Cost per egg trend data point.
/// </summary>
public sealed class CostPerEggTrendItemDto
{
    /// <summary>
    /// Date of the data point (YYYY-MM-DD).
    /// </summary>
    public string Date { get; set; } = string.Empty;

    /// <summary>
    /// Cost per egg on this date (cumulative).
    /// </summary>
    public decimal CostPerEgg { get; set; }
}

/// <summary>
/// Flock productivity data.
/// </summary>
public sealed class FlockProductivityItemDto
{
    /// <summary>
    /// Name of the flock.
    /// </summary>
    public string FlockName { get; set; } = string.Empty;

    /// <summary>
    /// Average eggs per hen per day for this flock.
    /// </summary>
    public decimal EggsPerHenPerDay { get; set; }

    /// <summary>
    /// Total eggs produced by this flock in the period.
    /// </summary>
    public int TotalEggs { get; set; }

    /// <summary>
    /// Number of hens in this flock.
    /// </summary>
    public int HenCount { get; set; }
}

/// <summary>
/// Monthly revenue vs. costs data point.
/// </summary>
public sealed class RevenueTrendItemDto
{
    /// <summary>
    /// Month of the data point (YYYY-MM).
    /// </summary>
    public string Month { get; set; } = string.Empty;

    /// <summary>
    /// Total egg sale revenue in this month.
    /// </summary>
    public decimal Revenue { get; set; }

    /// <summary>
    /// Total purchase costs in this month.
    /// </summary>
    public decimal Costs { get; set; }
}

/// <summary>
/// Summary statistics for the period.
/// </summary>
public sealed class StatisticsSummaryDto
{
    /// <summary>
    /// Total eggs produced in the period.
    /// </summary>
    public int TotalEggs { get; set; }

    /// <summary>
    /// Total costs in the period.
    /// </summary>
    public decimal TotalCost { get; set; }

    /// <summary>
    /// Average cost per egg.
    /// </summary>
    public decimal AvgCostPerEgg { get; set; }

    /// <summary>
    /// Average eggs per day.
    /// </summary>
    public decimal AvgEggsPerDay { get; set; }

    /// <summary>
    /// Total revenue from egg sales. Null if no sales recorded.
    /// </summary>
    public decimal? TotalRevenue { get; set; }

    /// <summary>
    /// Net profit/loss (revenue minus costs). Null if no sales recorded.
    /// </summary>
    public decimal? ProfitLoss { get; set; }
}
