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
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// Initializes a new instance of the <see cref="StatisticsRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="currentUserService">The current user service for tenant resolution.</param>
    public StatisticsRepository(ApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    private Guid GetTenantId() =>
        _currentUserService.TenantId ?? throw new InvalidOperationException("No tenant context");

    /// <summary>
    /// Gets aggregated dashboard statistics for the current tenant.
    /// Includes flock counts, today's eggs, weekly eggs, and cost per egg.
    /// </summary>
    /// <returns>Dashboard statistics containing coops, flocks, animal counts, production, and economics</returns>
    public async Task<DashboardStatsDto> GetDashboardStatsAsync()
    {
        var tenantId = GetTenantId();

        var today     = DateTime.UtcNow.Date;
        var tomorrow  = today.AddDays(1);
        var weekStart = today.AddDays(-6);

        // Query 1: flock aggregation (count, hens, roosters, chicks)
        var flockStats = await _context.Flocks
            .Where(f => f.TenantId == tenantId && f.IsActive)
            .GroupBy(f => 1)
            .Select(g => new
            {
                ActiveFlocks = g.Count(),
                TotalHens     = g.Sum(f => f.CurrentHens),
                TotalRoosters = g.Sum(f => f.CurrentRoosters),
                TotalChicks   = g.Sum(f => f.CurrentChicks),
                TotalAnimals  = g.Sum(f => f.CurrentHens + f.CurrentRoosters + f.CurrentChicks)
            })
            .FirstOrDefaultAsync();

        // Query 2: active coops count
        var totalCoops = await _context.Coops
            .Where(c => c.TenantId == tenantId && c.IsActive)
            .CountAsync();

        // Query 3: today's eggs, this week's eggs, and all-time eggs in a single aggregation.
        // Using conditional SUM (CASE WHEN) instead of three separate roundtrips.
        var eggStats = await _context.DailyRecords
            .Where(dr => dr.TenantId == tenantId)
            .GroupBy(dr => 1)
            .Select(g => new
            {
                TodayEggs    = g.Sum(dr => dr.RecordDate >= today    && dr.RecordDate < tomorrow ? (int?)dr.EggCount : null) ?? 0,
                ThisWeekEggs = g.Sum(dr => dr.RecordDate >= weekStart && dr.RecordDate < tomorrow ? (int?)dr.EggCount : null) ?? 0,
                TotalEggs    = g.Sum(dr => (int?)dr.EggCount) ?? 0
            })
            .FirstOrDefaultAsync();

        // Query 4: all-time purchase costs
        // Cast to double for SQLite compatibility (SQLite doesn't support decimal Sum)
        var totalCosts = (decimal)(await _context.Purchases
            .Where(p => p.TenantId == tenantId)
            .SumAsync(p => (double?)p.Amount) ?? 0.0);

        // Query 5: all-time egg sale revenue (Quantity × PricePerUnit)
        var totalRevenue = (decimal)(await _context.EggSales
            .Where(es => es.TenantId == tenantId)
            .SumAsync(es => (double?)(es.Quantity * es.PricePerUnit)) ?? 0.0);

        var todayEggs    = eggStats?.TodayEggs    ?? 0;
        var thisWeekEggs = eggStats?.ThisWeekEggs ?? 0;
        var totalEggs    = eggStats?.TotalEggs    ?? 0;

        var avgEggsPerDay = thisWeekEggs / 7m;
        decimal? costPerEgg = totalEggs > 0 ? totalCosts / totalEggs : null;

        return new DashboardStatsDto
        {
            TotalCoops    = totalCoops,
            ActiveFlocks  = flockStats?.ActiveFlocks  ?? 0,
            TotalHens     = flockStats?.TotalHens     ?? 0,
            TotalRoosters = flockStats?.TotalRoosters ?? 0,
            TotalChicks   = flockStats?.TotalChicks   ?? 0,
            TotalAnimals  = flockStats?.TotalAnimals  ?? 0,
            TodayEggs     = todayEggs,
            ThisWeekEggs  = thisWeekEggs,
            AvgEggsPerDay = avgEggsPerDay,
            CostPerEgg    = costPerEgg,
            TotalRevenue  = totalRevenue > 0 ? totalRevenue : (decimal?)null,
            ProfitLoss    = totalRevenue > 0 ? totalRevenue - totalCosts : (decimal?)null
        };
    }

    /// <summary>
    /// Gets detailed statistics for a given date range.
    /// Includes cost breakdown, production trends, cost per egg trends, and flock productivity.
    /// </summary>
    public async Task<StatisticsDto> GetStatisticsAsync(DateOnly? startDate = null, DateOnly? endDate = null, Guid? coopId = null, Guid? flockId = null)
    {
        // 1. Cost Breakdown by Purchase Type
        var costBreakdown = await GetCostBreakdownAsync(startDate, endDate, coopId);

        // 2. Production Trend (daily eggs)
        var productionTrend = await GetProductionTrendAsync(startDate, endDate, coopId, flockId);

        // 3. Cost Per Egg Trend (cumulative)
        var costPerEggTrend = await GetCostPerEggTrendAsync(startDate, endDate, coopId, flockId);

        // 4. Flock Productivity Comparison
        var flockProductivity = await GetFlockProductivityAsync(startDate, endDate, coopId, flockId);

        // 5. Revenue Trend (monthly revenue vs. costs)
        var revenueTrend = await GetRevenueTrendAsync(startDate, endDate);

        // 6. Summary Statistics
        var totalEggs = productionTrend.Sum(p => p.Eggs);
        var totalCost = costBreakdown.Sum(c => c.Amount);
        var avgCostPerEgg = totalEggs > 0 ? totalCost / totalEggs : 0;

        // Calculate avgEggsPerDay based on actual data range when no dates provided
        decimal avgEggsPerDay = 0;
        if (productionTrend.Count > 0)
        {
            int dayCount;
            if (startDate.HasValue && endDate.HasValue)
            {
                dayCount = (endDate.Value.ToDateTime(TimeOnly.MinValue) - startDate.Value.ToDateTime(TimeOnly.MinValue)).Days + 1;
            }
            else
            {
                var firstDate = DateOnly.Parse(productionTrend.First().Date);
                var lastDate = DateOnly.Parse(productionTrend.Last().Date);
                dayCount = (lastDate.ToDateTime(TimeOnly.MinValue) - firstDate.ToDateTime(TimeOnly.MinValue)).Days + 1;
            }
            avgEggsPerDay = dayCount > 0 ? (decimal)totalEggs / dayCount : 0;
        }

        var totalRevenue = revenueTrend.Any(r => r.Revenue > 0)
            ? (decimal?)revenueTrend.Sum(r => r.Revenue)
            : null;
        decimal? profitLoss = totalRevenue.HasValue ? totalRevenue.Value - totalCost : null;

        return new StatisticsDto
        {
            CostBreakdown = costBreakdown,
            ProductionTrend = productionTrend,
            CostPerEggTrend = costPerEggTrend,
            FlockProductivity = flockProductivity,
            RevenueTrend = revenueTrend,
            Summary = new StatisticsSummaryDto
            {
                TotalEggs = totalEggs,
                TotalCost = totalCost,
                AvgCostPerEgg = avgCostPerEgg,
                AvgEggsPerDay = avgEggsPerDay,
                TotalRevenue = totalRevenue,
                ProfitLoss = profitLoss
            }
        };
    }

    private async Task<List<CostBreakdownItemDto>> GetCostBreakdownAsync(DateOnly? startDate, DateOnly? endDate, Guid? coopId = null)
    {
        var tenantId = GetTenantId();
        var startDateTime = startDate.HasValue
            ? DateTime.SpecifyKind(startDate.Value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc)
            : (DateTime?)null;
        var endDateTime = endDate.HasValue
            ? DateTime.SpecifyKind(endDate.Value.ToDateTime(TimeOnly.MaxValue), DateTimeKind.Utc)
            : (DateTime?)null;

        var purchases = await _context.Purchases
            .Where(p => p.TenantId == tenantId)
            .Where(p => startDateTime == null || p.PurchaseDate >= startDateTime)
            .Where(p => endDateTime == null || p.PurchaseDate <= endDateTime)
            .Where(p => coopId == null || p.CoopId == coopId)
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

    private async Task<List<ProductionTrendItemDto>> GetProductionTrendAsync(DateOnly? startDate, DateOnly? endDate, Guid? coopId = null, Guid? flockId = null)
    {
        var tenantId = GetTenantId();
        var startDateTime = startDate.HasValue
            ? DateTime.SpecifyKind(startDate.Value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc)
            : (DateTime?)null;
        var endDateTime = endDate.HasValue
            ? DateTime.SpecifyKind(endDate.Value.ToDateTime(TimeOnly.MaxValue), DateTimeKind.Utc)
            : (DateTime?)null;

        var groups = await _context.DailyRecords
            .Where(dr => dr.TenantId == tenantId)
            .Where(dr => startDateTime == null || dr.RecordDate >= startDateTime)
            .Where(dr => endDateTime == null || dr.RecordDate <= endDateTime)
            .Where(dr => flockId == null || dr.FlockId == flockId)
            .Where(dr => coopId == null || dr.Flock!.CoopId == coopId)
            .GroupBy(dr => dr.RecordDate)
            .Select(g => new { Date = g.Key, Eggs = g.Sum(dr => dr.EggCount) })
            .OrderBy(g => g.Date)
            .ToListAsync();

        return groups.Select(g => new ProductionTrendItemDto
        {
            Date = g.Date.ToString("yyyy-MM-dd"),
            Eggs = g.Eggs
        }).ToList();
    }

    private async Task<List<CostPerEggTrendItemDto>> GetCostPerEggTrendAsync(DateOnly? startDate, DateOnly? endDate, Guid? coopId = null, Guid? flockId = null)
    {
        var tenantId = GetTenantId();
        var startDateTime = startDate.HasValue
            ? DateTime.SpecifyKind(startDate.Value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc)
            : (DateTime?)null;
        var endDateTime = endDate.HasValue
            ? DateTime.SpecifyKind(endDate.Value.ToDateTime(TimeOnly.MaxValue), DateTimeKind.Utc)
            : (DateTime?)null;

        // Get cumulative costs and eggs by date
        var dailyRecords = await _context.DailyRecords
            .Where(dr => dr.TenantId == tenantId)
            .Where(dr => startDateTime == null || dr.RecordDate >= startDateTime)
            .Where(dr => endDateTime == null || dr.RecordDate <= endDateTime)
            .Where(dr => flockId == null || dr.FlockId == flockId)
            .Where(dr => coopId == null || dr.Flock!.CoopId == coopId)
            .GroupBy(dr => dr.RecordDate)
            .Select(g => new
            {
                Date = g.Key,
                Eggs = g.Sum(dr => dr.EggCount)
            })
            .OrderBy(d => d.Date)
            .ToListAsync();

        var purchases = await _context.Purchases
            .Where(p => p.TenantId == tenantId)
            .Where(p => startDateTime == null || p.PurchaseDate >= startDateTime)
            .Where(p => endDateTime == null || p.PurchaseDate <= endDateTime)
            .Where(p => coopId == null || p.CoopId == coopId)
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

    private async Task<List<RevenueTrendItemDto>> GetRevenueTrendAsync(DateOnly? startDate, DateOnly? endDate)
    {
        var tenantId = GetTenantId();
        var startDateTime = startDate.HasValue
            ? DateTime.SpecifyKind(startDate.Value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc)
            : (DateTime?)null;
        var endDateTime = endDate.HasValue
            ? DateTime.SpecifyKind(endDate.Value.ToDateTime(TimeOnly.MaxValue), DateTimeKind.Utc)
            : (DateTime?)null;

        // Fetch minimal data client-side for month grouping (EF can't translate Year+Month grouping portably)
        var sales = await _context.EggSales
            .Where(es => es.TenantId == tenantId)
            .Where(es => startDateTime == null || es.Date >= startDateTime)
            .Where(es => endDateTime == null || es.Date <= endDateTime)
            .Select(es => new { es.Date, Revenue = es.Quantity * es.PricePerUnit })
            .ToListAsync();

        var purchases = await _context.Purchases
            .Where(p => p.TenantId == tenantId)
            .Where(p => startDateTime == null || p.PurchaseDate >= startDateTime)
            .Where(p => endDateTime == null || p.PurchaseDate <= endDateTime)
            .Select(p => new { p.PurchaseDate, p.Amount })
            .ToListAsync();

        var salesByMonth = sales
            .GroupBy(s => new { s.Date.Year, s.Date.Month })
            .ToDictionary(g => g.Key, g => g.Sum(s => s.Revenue));

        var costsByMonth = purchases
            .GroupBy(p => new { p.PurchaseDate.Year, p.PurchaseDate.Month })
            .ToDictionary(g => g.Key, g => g.Sum(p => p.Amount));

        var allMonths = salesByMonth.Keys
            .Union(costsByMonth.Keys)
            .Distinct()
            .OrderBy(m => m.Year)
            .ThenBy(m => m.Month)
            .ToList();

        return allMonths.Select(m => new RevenueTrendItemDto
        {
            Month = $"{m.Year:D4}-{m.Month:D2}",
            Revenue = salesByMonth.GetValueOrDefault(m, 0m),
            Costs = costsByMonth.GetValueOrDefault(m, 0m)
        }).ToList();
    }

    private async Task<List<FlockProductivityItemDto>> GetFlockProductivityAsync(DateOnly? startDate, DateOnly? endDate, Guid? coopId = null, Guid? flockId = null)
    {
        var tenantId = GetTenantId();
        var startDateTime = startDate.HasValue
            ? DateTime.SpecifyKind(startDate.Value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc)
            : (DateTime?)null;
        var endDateTime = endDate.HasValue
            ? DateTime.SpecifyKind(endDate.Value.ToDateTime(TimeOnly.MaxValue), DateTimeKind.Utc)
            : (DateTime?)null;

        var flockStats = await _context.DailyRecords
            .Where(dr => dr.TenantId == tenantId)
            .Where(dr => startDateTime == null || dr.RecordDate >= startDateTime)
            .Where(dr => endDateTime == null || dr.RecordDate <= endDateTime)
            .Where(dr => flockId == null || dr.FlockId == flockId)
            .Where(dr => coopId == null || dr.Flock!.CoopId == coopId)
            .Include(dr => dr.Flock)
            .GroupBy(dr => new { dr.FlockId, dr.Flock!.Identifier })
            .Select(g => new
            {
                FlockName = g.Key.Identifier,
                TotalEggs = g.Sum(dr => dr.EggCount),
                HenCount = g.Max(dr => dr.Flock!.CurrentHens) // Get current hen count
            })
            .ToListAsync();

        var dayCount = startDate.HasValue && endDate.HasValue
            ? (endDate.Value.ToDateTime(TimeOnly.MinValue) - startDate.Value.ToDateTime(TimeOnly.MinValue)).Days + 1
            : (flockStats.Count > 0 ? 1 : 0); // For all-time, use 1 day as minimum to avoid division by zero

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
