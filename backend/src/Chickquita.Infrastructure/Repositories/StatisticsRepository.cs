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

    /// <summary>
    /// Gets detailed statistics for a given date range.
    /// Includes cost breakdown, production trends, cost per egg trends, and flock productivity.
    /// Tenant isolation is enforced by Row-Level Security (RLS) at the database level.
    /// </summary>
    public async Task<StatisticsDto> GetStatisticsAsync(DateOnly startDate, DateOnly endDate)
    {
        // 1. Cost Breakdown by Purchase Type
        var costBreakdown = await GetCostBreakdownAsync(startDate, endDate);

        // 2. Production Trend (daily eggs)
        var productionTrend = await GetProductionTrendAsync(startDate, endDate);

        // 3. Cost Per Egg Trend (cumulative)
        var costPerEggTrend = await GetCostPerEggTrendAsync(startDate, endDate);

        // 4. Flock Productivity Comparison
        var flockProductivity = await GetFlockProductivityAsync(startDate, endDate);

        // 5. Summary Statistics
        var totalEggs = productionTrend.Sum(p => p.Eggs);
        var totalCost = costBreakdown.Sum(c => c.Amount);
        var avgCostPerEgg = totalEggs > 0 ? totalCost / totalEggs : 0;
        var dayCount = (endDate.ToDateTime(TimeOnly.MinValue) - startDate.ToDateTime(TimeOnly.MinValue)).Days + 1;
        var avgEggsPerDay = dayCount > 0 ? (decimal)totalEggs / dayCount : 0;

        return new StatisticsDto
        {
            CostBreakdown = costBreakdown,
            ProductionTrend = productionTrend,
            CostPerEggTrend = costPerEggTrend,
            FlockProductivity = flockProductivity,
            Summary = new StatisticsSummaryDto
            {
                TotalEggs = totalEggs,
                TotalCost = totalCost,
                AvgCostPerEgg = avgCostPerEgg,
                AvgEggsPerDay = avgEggsPerDay
            }
        };
    }

    private async Task<List<CostBreakdownItemDto>> GetCostBreakdownAsync(DateOnly startDate, DateOnly endDate)
    {
        var startDateTime = DateTime.SpecifyKind(startDate.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
        var endDateTime = DateTime.SpecifyKind(endDate.ToDateTime(TimeOnly.MaxValue), DateTimeKind.Utc);

        var purchases = await _context.Purchases
            .Where(p => p.PurchaseDate >= startDateTime && p.PurchaseDate <= endDateTime)
            .GroupBy(p => p.Type)
            .Select(g => new
            {
                Type = g.Key,
                Amount = g.Sum(p => p.Amount)
            })
            .ToListAsync();

        var totalAmount = purchases.Sum(p => p.Amount);

        return purchases.Select(p => new CostBreakdownItemDto
        {
            Type = p.Type.ToString(),
            Amount = p.Amount,
            Percentage = totalAmount > 0 ? (p.Amount / totalAmount) * 100 : 0
        }).ToList();
    }

    private async Task<List<ProductionTrendItemDto>> GetProductionTrendAsync(DateOnly startDate, DateOnly endDate)
    {
        var startDateTime = DateTime.SpecifyKind(startDate.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
        var endDateTime = DateTime.SpecifyKind(endDate.ToDateTime(TimeOnly.MaxValue), DateTimeKind.Utc);

        return await _context.DailyRecords
            .Where(dr => dr.RecordDate >= startDateTime && dr.RecordDate <= endDateTime)
            .GroupBy(dr => dr.RecordDate)
            .Select(g => new ProductionTrendItemDto
            {
                Date = g.Key.ToString("yyyy-MM-dd"),
                Eggs = g.Sum(dr => dr.EggCount)
            })
            .OrderBy(p => p.Date)
            .ToListAsync();
    }

    private async Task<List<CostPerEggTrendItemDto>> GetCostPerEggTrendAsync(DateOnly startDate, DateOnly endDate)
    {
        var startDateTime = DateTime.SpecifyKind(startDate.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
        var endDateTime = DateTime.SpecifyKind(endDate.ToDateTime(TimeOnly.MaxValue), DateTimeKind.Utc);

        // Get cumulative costs and eggs by date
        var dailyRecords = await _context.DailyRecords
            .Where(dr => dr.RecordDate >= startDateTime && dr.RecordDate <= endDateTime)
            .GroupBy(dr => dr.RecordDate)
            .Select(g => new
            {
                Date = g.Key,
                Eggs = g.Sum(dr => dr.EggCount)
            })
            .OrderBy(d => d.Date)
            .ToListAsync();

        var purchases = await _context.Purchases
            .Where(p => p.PurchaseDate >= startDateTime && p.PurchaseDate <= endDateTime)
            .GroupBy(p => p.PurchaseDate)
            .Select(g => new
            {
                Date = g.Key,
                Cost = g.Sum(p => p.Amount)
            })
            .ToListAsync();

        // Calculate cumulative cost per egg
        var result = new List<CostPerEggTrendItemDto>();
        decimal cumulativeCost = 0;
        int cumulativeEggs = 0;

        var allDates = dailyRecords.Select(d => d.Date)
            .Union(purchases.Select(p => p.Date))
            .Distinct()
            .OrderBy(d => d)
            .ToList();

        foreach (var date in allDates)
        {
            cumulativeCost += purchases.FirstOrDefault(p => p.Date == date)?.Cost ?? 0;
            cumulativeEggs += dailyRecords.FirstOrDefault(d => d.Date == date)?.Eggs ?? 0;

            if (cumulativeEggs > 0)
            {
                result.Add(new CostPerEggTrendItemDto
                {
                    Date = date.ToString("yyyy-MM-dd"),
                    CostPerEgg = cumulativeCost / cumulativeEggs
                });
            }
        }

        return result;
    }

    private async Task<List<FlockProductivityItemDto>> GetFlockProductivityAsync(DateOnly startDate, DateOnly endDate)
    {
        var startDateTime = DateTime.SpecifyKind(startDate.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
        var endDateTime = DateTime.SpecifyKind(endDate.ToDateTime(TimeOnly.MaxValue), DateTimeKind.Utc);

        var flockStats = await _context.DailyRecords
            .Where(dr => dr.RecordDate >= startDateTime && dr.RecordDate <= endDateTime)
            .Include(dr => dr.Flock)
            .GroupBy(dr => new { dr.FlockId, dr.Flock!.Identifier })
            .Select(g => new
            {
                FlockName = g.Key.Identifier,
                TotalEggs = g.Sum(dr => dr.EggCount),
                HenCount = g.Max(dr => dr.Flock!.CurrentHens) // Get current hen count
            })
            .ToListAsync();

        var dayCount = (endDate.ToDateTime(TimeOnly.MinValue) - startDate.ToDateTime(TimeOnly.MinValue)).Days + 1;

        return flockStats.Select(f => new FlockProductivityItemDto
        {
            FlockName = f.FlockName,
            TotalEggs = f.TotalEggs,
            HenCount = f.HenCount,
            EggsPerHenPerDay = f.HenCount > 0 && dayCount > 0
                ? (decimal)f.TotalEggs / (f.HenCount * dayCount)
                : 0
        }).ToList();
    }
}
