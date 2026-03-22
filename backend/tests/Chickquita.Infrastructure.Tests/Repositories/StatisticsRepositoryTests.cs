using Chickquita.Application.Interfaces;
using Chickquita.Domain.Entities;
using Chickquita.Infrastructure.Data;
using Chickquita.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Moq;
using Xunit;

namespace Chickquita.Infrastructure.Tests.Repositories;

/// <summary>
/// Integration tests for StatisticsRepository.
/// Verifies aggregation queries, tenant isolation, and dashboard statistics calculations.
/// </summary>
public class StatisticsRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ApplicationDbContext _dbContext;
    private readonly StatisticsRepository _repository;
    private readonly Guid _tenantId;
    private readonly Guid _coopId;
    private readonly Guid _flockId;

    /// <summary>
    /// SQLite-compatible subclass — converts TimeSpan? to ticks so SQLite can handle the column.
    /// </summary>
    private class SqliteApplicationDbContext : ApplicationDbContext
    {
        public SqliteApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            ICurrentUserService currentUserService)
            : base(options, currentUserService)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DailyRecord>()
                .Property(d => d.CollectionTime)
                .HasColumnType("INTEGER")
                .HasConversion(
                    v => v.HasValue ? v.Value.Ticks : (long?)null,
                    v => v.HasValue ? TimeSpan.FromTicks(v.Value) : (TimeSpan?)null);

            // SQLite doesn't support SUM on decimal (TEXT) columns — map all decimals to REAL
            var decimalConverter = new ValueConverter<decimal, double>(v => (double)v, v => (decimal)v);
            var nullableDecimalConverter = new ValueConverter<decimal?, double?>(
                v => v.HasValue ? (double?)v.Value : null,
                v => v.HasValue ? (decimal?)v.Value : null);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(decimal))
                    {
                        property.SetColumnType("REAL");
                        property.SetValueConverter(decimalConverter);
                    }
                    else if (property.ClrType == typeof(decimal?))
                    {
                        property.SetColumnType("REAL");
                        property.SetValueConverter(nullableDecimalConverter);
                    }
                }
            }
        }
    }

    public StatisticsRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        _tenantId = Guid.NewGuid();

        var mockCurrentUserService = new Mock<ICurrentUserService>();
        mockCurrentUserService.Setup(x => x.TenantId).Returns(_tenantId);

        _dbContext = new SqliteApplicationDbContext(options, mockCurrentUserService.Object);
        _dbContext.Database.EnsureCreated();

        _repository = new StatisticsRepository(_dbContext, mockCurrentUserService.Object);

        var tenant = Tenant.Create("clerk_stats_test", "stats@example.com").Value;
        typeof(Tenant).GetProperty(nameof(Tenant.Id))!.SetValue(tenant, _tenantId);
        _dbContext.Tenants.Add(tenant);

        var coop = Coop.Create(_tenantId, "Test Coop", "Test Location").Value;
        _dbContext.Coops.Add(coop);
        _dbContext.SaveChanges();
        _coopId = coop.Id;

        var flock = Flock.Create(_tenantId, _coopId, "FLOCK-A", DateTime.UtcNow.AddMonths(-3), 20, 2, 5, null).Value;
        _dbContext.Flocks.Add(flock);
        _dbContext.SaveChanges();
        _flockId = flock.Id;
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }

    #region GetDashboardStatsAsync — egg aggregation

    [Fact]
    public async Task GetDashboardStatsAsync_WithNoRecords_ReturnsZeroEggs()
    {
        var result = await _repository.GetDashboardStatsAsync();

        result.TodayEggs.Should().Be(0);
        result.ThisWeekEggs.Should().Be(0);
        result.AvgEggsPerDay.Should().Be(0);
        result.CostPerEgg.Should().BeNull();
    }

    [Fact]
    public async Task GetDashboardStatsAsync_TodayRecord_CountsInTodayAndWeek()
    {
        var today = DateTime.UtcNow.Date;
        var record = DailyRecord.Create(_tenantId, _flockId, today, 15, null).Value;
        _dbContext.DailyRecords.Add(record);
        await _dbContext.SaveChangesAsync();

        var result = await _repository.GetDashboardStatsAsync();

        result.TodayEggs.Should().Be(15);
        result.ThisWeekEggs.Should().Be(15);
    }

    [Fact]
    public async Task GetDashboardStatsAsync_OldRecord_NotCountedInTodayOrWeek()
    {
        var today = DateTime.UtcNow.Date;
        var old = DailyRecord.Create(_tenantId, _flockId, today.AddDays(-10), 50, null).Value;
        var todayRec = DailyRecord.Create(_tenantId, _flockId, today, 8, null).Value;
        _dbContext.DailyRecords.AddRange(old, todayRec);
        await _dbContext.SaveChangesAsync();

        var result = await _repository.GetDashboardStatsAsync();

        result.TodayEggs.Should().Be(8);
        result.ThisWeekEggs.Should().Be(8);  // old record is outside 7-day window
    }

    [Fact]
    public async Task GetDashboardStatsAsync_WeekRecord_CountedInWeekButNotToday()
    {
        var today = DateTime.UtcNow.Date;
        var yesterday = today.AddDays(-1);
        var rec = DailyRecord.Create(_tenantId, _flockId, yesterday, 12, null).Value;
        _dbContext.DailyRecords.Add(rec);
        await _dbContext.SaveChangesAsync();

        var result = await _repository.GetDashboardStatsAsync();

        result.TodayEggs.Should().Be(0);
        result.ThisWeekEggs.Should().Be(12);
    }

    [Fact]
    public async Task GetDashboardStatsAsync_MultipleRecordsThisWeek_SumsCorrectly()
    {
        var today = DateTime.UtcNow.Date;
        var records = Enumerable.Range(0, 5).Select(i =>
            DailyRecord.Create(_tenantId, _flockId, today.AddDays(-i), 10, null).Value
        ).ToList();
        _dbContext.DailyRecords.AddRange(records);
        await _dbContext.SaveChangesAsync();

        var result = await _repository.GetDashboardStatsAsync();

        result.TodayEggs.Should().Be(10);
        result.ThisWeekEggs.Should().Be(50);
        result.AvgEggsPerDay.Should().Be(50m / 7m);
    }

    [Fact]
    public async Task GetDashboardStatsAsync_RecordAtWindowBoundary_Included()
    {
        var today = DateTime.UtcNow.Date;
        var boundary = today.AddDays(-6); // first day of 7-day window
        var rec = DailyRecord.Create(_tenantId, _flockId, boundary, 10, null).Value;
        _dbContext.DailyRecords.Add(rec);
        await _dbContext.SaveChangesAsync();

        var result = await _repository.GetDashboardStatsAsync();

        result.ThisWeekEggs.Should().Be(10);
    }

    [Fact]
    public async Task GetDashboardStatsAsync_RecordJustOutsideWindow_NotIncluded()
    {
        var today = DateTime.UtcNow.Date;
        var outside = today.AddDays(-7); // one day before window starts
        var rec = DailyRecord.Create(_tenantId, _flockId, outside, 10, null).Value;
        _dbContext.DailyRecords.Add(rec);
        await _dbContext.SaveChangesAsync();

        var result = await _repository.GetDashboardStatsAsync();

        result.ThisWeekEggs.Should().Be(0);
    }

    #endregion

    #region GetDashboardStatsAsync — CostPerEgg

    [Fact]
    public async Task GetDashboardStatsAsync_WithCostsAndEggs_ComputesCostPerEgg()
    {
        var today = DateTime.UtcNow.Date;
        var record = DailyRecord.Create(_tenantId, _flockId, today, 100, null).Value;
        _dbContext.DailyRecords.Add(record);

        var purchase = Purchase.Create(
            _tenantId, "Feed", PurchaseType.Feed, 50m, 10m, QuantityUnit.Kg,
            today, _coopId).Value;
        _dbContext.Purchases.Add(purchase);
        await _dbContext.SaveChangesAsync();

        var result = await _repository.GetDashboardStatsAsync();

        result.CostPerEgg.Should().Be(0.5m);
    }

    [Fact]
    public async Task GetDashboardStatsAsync_WithCostsButNoEggs_CostPerEggIsNull()
    {
        var purchase = Purchase.Create(
            _tenantId, "Feed", PurchaseType.Feed, 100m, 10m, QuantityUnit.Kg,
            DateTime.UtcNow, _coopId).Value;
        _dbContext.Purchases.Add(purchase);
        await _dbContext.SaveChangesAsync();

        var result = await _repository.GetDashboardStatsAsync();

        result.CostPerEgg.Should().BeNull();
    }

    #endregion

    #region GetDashboardStatsAsync — flock/coop stats

    [Fact]
    public async Task GetDashboardStatsAsync_ReturnsCorrectFlockAndCoopCounts()
    {
        var result = await _repository.GetDashboardStatsAsync();

        result.TotalCoops.Should().Be(1);
        result.ActiveFlocks.Should().Be(1);
        result.TotalHens.Should().Be(20);
        result.TotalRoosters.Should().Be(2);
        result.TotalChicks.Should().Be(5);
        result.TotalAnimals.Should().Be(27);
    }

    [Fact]
    public async Task GetDashboardStatsAsync_InactiveFlock_NotCounted()
    {
        var inactiveFlock = Flock.Create(_tenantId, _coopId, "INACTIVE", DateTime.UtcNow.AddMonths(-6), 15, 1, 0, null).Value;
        inactiveFlock.Archive();
        _dbContext.Flocks.Add(inactiveFlock);
        await _dbContext.SaveChangesAsync();

        var result = await _repository.GetDashboardStatsAsync();

        result.ActiveFlocks.Should().Be(1);  // only the original flock
    }

    #endregion

    #region GetDashboardStatsAsync — TotalRevenue / ProfitLoss

    [Fact]
    public async Task GetDashboardStatsAsync_WithNoSales_TotalRevenueAndProfitLossAreNull()
    {
        var result = await _repository.GetDashboardStatsAsync();

        result.TotalRevenue.Should().BeNull();
        result.ProfitLoss.Should().BeNull();
    }

    [Fact]
    public async Task GetDashboardStatsAsync_WithEggSales_ComputesTotalRevenue()
    {
        var today = DateTime.UtcNow.Date;
        var sale1 = EggSale.Create(_tenantId, today, 100, 5.0m).Value;   // 500
        var sale2 = EggSale.Create(_tenantId, today.AddDays(-1), 50, 4.0m).Value; // 200
        _dbContext.EggSales.AddRange(sale1, sale2);
        await _dbContext.SaveChangesAsync();

        var result = await _repository.GetDashboardStatsAsync();

        result.TotalRevenue.Should().BeApproximately(700m, 0.01m);
    }

    [Fact]
    public async Task GetDashboardStatsAsync_WithSalesAndCosts_ComputesProfitLoss()
    {
        var today = DateTime.UtcNow.Date;
        var sale = EggSale.Create(_tenantId, today, 200, 5.0m).Value; // 1000 revenue
        _dbContext.EggSales.Add(sale);

        var purchase = Purchase.Create(
            _tenantId, "Feed", PurchaseType.Feed, 300m, 10m, QuantityUnit.Kg,
            today, _coopId).Value;
        _dbContext.Purchases.Add(purchase);
        await _dbContext.SaveChangesAsync();

        var result = await _repository.GetDashboardStatsAsync();

        result.TotalRevenue.Should().BeApproximately(1000m, 0.01m);
        result.ProfitLoss.Should().BeApproximately(700m, 0.01m); // 1000 - 300
    }

    [Fact]
    public async Task GetDashboardStatsAsync_WithSalesExceedingCosts_PositiveProfitLoss()
    {
        var today = DateTime.UtcNow.Date;
        var sale = EggSale.Create(_tenantId, today, 100, 10.0m).Value; // 1000 revenue
        _dbContext.EggSales.Add(sale);

        var purchase = Purchase.Create(
            _tenantId, "Feed", PurchaseType.Feed, 400m, 20m, QuantityUnit.Kg,
            today, _coopId).Value;
        _dbContext.Purchases.Add(purchase);
        await _dbContext.SaveChangesAsync();

        var result = await _repository.GetDashboardStatsAsync();

        result.ProfitLoss.Should().BePositive();
    }

    [Fact]
    public async Task GetDashboardStatsAsync_WithCostsExceedingSales_NegativeProfitLoss()
    {
        var today = DateTime.UtcNow.Date;
        var sale = EggSale.Create(_tenantId, today, 10, 1.0m).Value; // 10 revenue
        _dbContext.EggSales.Add(sale);

        var purchase = Purchase.Create(
            _tenantId, "Feed", PurchaseType.Feed, 500m, 20m, QuantityUnit.Kg,
            today, _coopId).Value;
        _dbContext.Purchases.Add(purchase);
        await _dbContext.SaveChangesAsync();

        var result = await _repository.GetDashboardStatsAsync();

        result.ProfitLoss.Should().BeNegative();
    }

    #endregion

    #region GetDashboardStatsAsync — tenant isolation

    [Fact]
    public async Task GetDashboardStatsAsync_OtherTenantData_NotIncluded()
    {
        var otherTenantId = Guid.NewGuid();
        var today = DateTime.UtcNow.Date;

        // Seed other tenant's data directly (bypassing RLS filter)
        var otherTenant = Tenant.Create("other_user", "other@example.com").Value;
        typeof(Tenant).GetProperty(nameof(Tenant.Id))!.SetValue(otherTenant, otherTenantId);
        _dbContext.Tenants.Add(otherTenant);

        var otherCoop = Coop.Create(otherTenantId, "Other Coop", null).Value;
        _dbContext.Coops.Add(otherCoop);
        await _dbContext.SaveChangesAsync();

        var otherFlock = Flock.Create(otherTenantId, otherCoop.Id, "OTHER-FLOCK", DateTime.UtcNow.AddMonths(-1), 50, 5, 10, null).Value;
        _dbContext.Flocks.Add(otherFlock);
        await _dbContext.SaveChangesAsync();

        var otherRecord = DailyRecord.Create(otherTenantId, otherFlock.Id, today, 200, null).Value;
        _dbContext.DailyRecords.Add(otherRecord);
        await _dbContext.SaveChangesAsync();

        var result = await _repository.GetDashboardStatsAsync();

        result.TodayEggs.Should().Be(0);
        result.ActiveFlocks.Should().Be(1);  // only our flock
        result.TotalCoops.Should().Be(1);    // only our coop
    }

    #endregion
}
