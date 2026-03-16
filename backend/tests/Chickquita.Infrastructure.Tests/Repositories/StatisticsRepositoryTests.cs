using Chickquita.Application.Interfaces;
using Chickquita.Domain.Entities;
using Chickquita.Infrastructure.Data;
using Chickquita.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Chickquita.Infrastructure.Tests.Repositories;

/// <summary>
/// Integration tests for StatisticsRepository.GetDashboardStatsAsync.
/// Verifies that today/week/all-time egg counts are calculated correctly
/// using a single consolidated DB query.
/// </summary>
public class StatisticsRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ApplicationDbContext _dbContext;
    private readonly StatisticsRepository _repository;
    private readonly Guid _tenantId;
    private readonly Guid _coopId;
    private readonly Guid _flockId;

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

        var tenant = Tenant.Create("clerk_user_test", "test@example.com");
        typeof(Tenant).GetProperty(nameof(Tenant.Id))!.SetValue(tenant, _tenantId);
        _dbContext.Tenants.Add(tenant);

        var coop = Coop.Create(_tenantId, "Test Coop", "Test Location");
        _dbContext.Coops.Add(coop);
        _dbContext.SaveChanges();
        _coopId = coop.Id;

        var flock = Flock.Create(_tenantId, _coopId, "TEST-FLOCK", DateTime.UtcNow.AddMonths(-3), 20, 2, 0, null);
        _dbContext.Flocks.Add(flock);
        _dbContext.SaveChanges();
        _flockId = flock.Id;
    }

    #region GetDashboardStatsAsync — egg consolidation tests

    [Fact]
    public async Task GetDashboardStatsAsync_WithNoRecords_ReturnsZeroEggs()
    {
        // Act
        var result = await _repository.GetDashboardStatsAsync();

        // Assert
        result.TodayEggs.Should().Be(0);
        result.ThisWeekEggs.Should().Be(0);
    }

    [Fact]
    public async Task GetDashboardStatsAsync_WithTodayRecord_ReturnsTodayEggs()
    {
        // Arrange
        var today = DateTime.UtcNow.Date;
        _dbContext.DailyRecords.Add(DailyRecord.Create(_tenantId, _flockId, today, 15, null));
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetDashboardStatsAsync();

        // Assert
        result.TodayEggs.Should().Be(15);
        result.ThisWeekEggs.Should().Be(15);
    }

    [Fact]
    public async Task GetDashboardStatsAsync_WithYesterdayRecord_ExcludesTodayEggs()
    {
        // Arrange
        var yesterday = DateTime.UtcNow.Date.AddDays(-1);
        _dbContext.DailyRecords.Add(DailyRecord.Create(_tenantId, _flockId, yesterday, 10, null));
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetDashboardStatsAsync();

        // Assert
        result.TodayEggs.Should().Be(0);
        result.ThisWeekEggs.Should().Be(10);
    }

    [Fact]
    public async Task GetDashboardStatsAsync_WithRecordsSpanningLastWeek_AccumulatesWeekEggs()
    {
        // Arrange — 3 records within the last 7 days, 1 older than 7 days
        var today = DateTime.UtcNow.Date;
        _dbContext.DailyRecords.AddRange(
            DailyRecord.Create(_tenantId, _flockId, today, 10, null),
            DailyRecord.Create(_tenantId, _flockId, today.AddDays(-3), 20, null),
            DailyRecord.Create(_tenantId, _flockId, today.AddDays(-6), 30, null),
            DailyRecord.Create(_tenantId, _flockId, today.AddDays(-7), 99, null) // outside window
        );
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetDashboardStatsAsync();

        // Assert
        result.TodayEggs.Should().Be(10);
        result.ThisWeekEggs.Should().Be(60);  // 10 + 20 + 30 — not 99
    }

    [Fact]
    public async Task GetDashboardStatsAsync_CostPerEgg_UsesAllTimeEggs()
    {
        // Arrange — two records: one today, one 30 days ago
        var today = DateTime.UtcNow.Date;
        _dbContext.DailyRecords.AddRange(
            DailyRecord.Create(_tenantId, _flockId, today, 5, null),
            DailyRecord.Create(_tenantId, _flockId, today.AddDays(-30), 95, null)
        );

        // Add a purchase for total cost
        var purchase = Purchase.Create(_tenantId, "Test Feed", PurchaseType.Feed, 200m, 10m, QuantityUnit.Kg, today, _coopId);
        _dbContext.Purchases.Add(purchase);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetDashboardStatsAsync();

        // Assert — all-time eggs = 100 (5 + 95), cost = 200, so cost/egg = 2
        result.CostPerEgg.Should().Be(2m);
        result.TodayEggs.Should().Be(5);
    }

    [Fact]
    public async Task GetDashboardStatsAsync_WithMultipleFlocks_SumsEggsAcrossFlocks()
    {
        // Arrange — add a second flock and records for both
        var flock2 = Flock.Create(_tenantId, _coopId, "TEST-FLOCK-2", DateTime.UtcNow.AddMonths(-2), 10, 1, 0, null);
        _dbContext.Flocks.Add(flock2);
        await _dbContext.SaveChangesAsync();

        var today = DateTime.UtcNow.Date;
        _dbContext.DailyRecords.AddRange(
            DailyRecord.Create(_tenantId, _flockId, today, 8, null),
            DailyRecord.Create(_tenantId, flock2.Id, today, 12, null)
        );
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetDashboardStatsAsync();

        // Assert
        result.TodayEggs.Should().Be(20);
        result.ThisWeekEggs.Should().Be(20);
    }

    #endregion

    #region GetDashboardStatsAsync — flock and coop counts

    [Fact]
    public async Task GetDashboardStatsAsync_WithActiveCoopAndFlock_ReturnsCorrectCounts()
    {
        // Act
        var result = await _repository.GetDashboardStatsAsync();

        // Assert
        result.TotalCoops.Should().Be(1);
        result.ActiveFlocks.Should().Be(1);
        result.TotalHens.Should().Be(20);
        result.TotalRoosters.Should().Be(2);
        result.TotalChicks.Should().Be(0);
    }

    #endregion

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }
}
