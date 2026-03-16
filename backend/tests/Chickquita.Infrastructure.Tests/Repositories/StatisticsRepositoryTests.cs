using Chickquita.Application.Interfaces;
using Chickquita.Domain.Entities;
using Chickquita.Infrastructure.Data;
using Chickquita.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
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
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties()
                    .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
                {
                    property.SetColumnType("REAL");
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

        var tenant = Tenant.Create("clerk_stats_test", "stats@example.com");
        typeof(Tenant).GetProperty(nameof(Tenant.Id))!.SetValue(tenant, _tenantId);
        _dbContext.Tenants.Add(tenant);

        var coop = Coop.Create(_tenantId, "Test Coop", "Test Location");
        _dbContext.Coops.Add(coop);
        _dbContext.SaveChanges();
        _coopId = coop.Id;

        var flock = Flock.Create(_tenantId, _coopId, "FLOCK-A", DateTime.UtcNow.AddMonths(-3), 20, 2, 5, null);
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
        var record = DailyRecord.Create(_tenantId, _flockId, today, 15, null);
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
        var old = DailyRecord.Create(_tenantId, _flockId, today.AddDays(-10), 50, null);
        var todayRec = DailyRecord.Create(_tenantId, _flockId, today, 8, null);
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
        var rec = DailyRecord.Create(_tenantId, _flockId, yesterday, 12, null);
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
            DailyRecord.Create(_tenantId, _flockId, today.AddDays(-i), 10, null)
        ).ToList();
        _dbContext.DailyRecords.AddRange(records);
        await _dbContext.SaveChangesAsync();

        var result = await _repository.GetDashboardStatsAsync();

        result.TodayEggs.Should().Be(10);
        result.ThisWeekEggs.Should().Be(50);
        result.AvgEggsPerDay.Should().Be(50m / 7m);
    }

    #endregion

    #region GetDashboardStatsAsync — CostPerEgg

    [Fact]
    public async Task GetDashboardStatsAsync_WithCostsAndEggs_ComputesCostPerEgg()
    {
        var today = DateTime.UtcNow.Date;
        var record = DailyRecord.Create(_tenantId, _flockId, today, 100, null);
        _dbContext.DailyRecords.Add(record);

        var purchase = Purchase.Create(
            _tenantId, "Feed", PurchaseType.Feed, 50m, 10m, QuantityUnit.Kg,
            today, _coopId);
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
            DateTime.UtcNow, _coopId);
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
        var inactiveFlock = Flock.Create(_tenantId, _coopId, "INACTIVE", DateTime.UtcNow.AddMonths(-6), 15, 1, 0, null);
        inactiveFlock.Archive();
        _dbContext.Flocks.Add(inactiveFlock);
        await _dbContext.SaveChangesAsync();

        var result = await _repository.GetDashboardStatsAsync();

        result.ActiveFlocks.Should().Be(1);  // only the original flock
    }

    #endregion

    #region GetDashboardStatsAsync — tenant isolation

    [Fact]
    public async Task GetDashboardStatsAsync_OtherTenantData_NotIncluded()
    {
        var otherTenantId = Guid.NewGuid();
        var today = DateTime.UtcNow.Date;

        // Seed other tenant's data directly (bypassing RLS filter)
        var otherTenant = Tenant.Create("other_user", "other@example.com");
        typeof(Tenant).GetProperty(nameof(Tenant.Id))!.SetValue(otherTenant, otherTenantId);
        _dbContext.Tenants.Add(otherTenant);

        var otherCoop = Coop.Create(otherTenantId, "Other Coop", null);
        _dbContext.Coops.Add(otherCoop);
        await _dbContext.SaveChangesAsync();

        var otherFlock = Flock.Create(otherTenantId, otherCoop.Id, "OTHER-FLOCK", DateTime.UtcNow.AddMonths(-1), 50, 5, 10, null);
        _dbContext.Flocks.Add(otherFlock);
        await _dbContext.SaveChangesAsync();

        var otherRecord = DailyRecord.Create(otherTenantId, otherFlock.Id, today, 200, null);
        _dbContext.DailyRecords.Add(otherRecord);
        await _dbContext.SaveChangesAsync();

        var result = await _repository.GetDashboardStatsAsync();

        result.TodayEggs.Should().Be(0);
        result.ActiveFlocks.Should().Be(1);  // only our flock
        result.TotalCoops.Should().Be(1);    // only our coop
    }

    #endregion
}
