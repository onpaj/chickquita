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
/// Integration tests for FlockRepository.
/// Uses SQLite in-memory database to verify that UpdateAsync correctly handles
/// EF Core change tracking, especially for FlockHistory child entities.
///
/// Each test uses a fresh DbContext per operation to mirror real request-scoped usage
/// and avoid EF Core identity-map interference.
/// </summary>
public class FlockRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<ApplicationDbContext> _options;
    private readonly Guid _tenantId;
    private readonly Guid _coopId;

    public FlockRepositoryTests()
    {
        // Keep the connection open for the lifetime of the test so in-memory DB persists
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        _tenantId = Guid.NewGuid();

        // Seed tenant and coop using a setup context
        using var setupContext = CreateContext();
        setupContext.Database.EnsureCreated();

        var tenant = Tenant.Create("clerk_user_test", "test@example.com");
        typeof(Tenant).GetProperty(nameof(Tenant.Id))!.SetValue(tenant, _tenantId);
        setupContext.Tenants.Add(tenant);

        var coop = Coop.Create(_tenantId, "Test Coop", null);
        setupContext.Coops.Add(coop);
        setupContext.SaveChanges();
        _coopId = coop.Id;
    }

    public void Dispose()
    {
        _connection.Dispose();
    }

    /// <summary>Creates a fresh DbContext that scopes to _tenantId.</summary>
    private ApplicationDbContext CreateContext()
    {
        var mockCurrentUser = new Mock<ICurrentUserService>();
        mockCurrentUser.Setup(x => x.TenantId).Returns(_tenantId);
        return new ApplicationDbContext(_options, mockCurrentUser.Object);
    }

    [Fact]
    public async Task UpdateAsync_WhenEntityIsTracked_ShouldSaveChangesWithoutConflict()
    {
        // Arrange: persist a flock in a dedicated context
        var flockId = Guid.Empty;
        using (var ctx = CreateContext())
        {
            var repo = new FlockRepository(ctx);
            var flock = Flock.Create(_tenantId, _coopId, "Original", DateTime.UtcNow.AddDays(-30), 10, 2, 0);
            await repo.AddAsync(flock);
            await ctx.SaveChangesAsync();
            flockId = flock.Id;
        }

        // Act: load and update in a fresh context (simulating a new request)
        using (var ctx = CreateContext())
        {
            var repo = new FlockRepository(ctx);
            var trackedFlock = await repo.GetByIdWithoutHistoryAsync(flockId);
            trackedFlock.Should().NotBeNull();

            trackedFlock!.Update("Updated Name", DateTime.UtcNow.AddDays(-20));

            // UpdateAsync should detect the entity is already tracked and skip Update()
            var result = await repo.UpdateAsync(trackedFlock);
            await ctx.SaveChangesAsync();
            result.Identifier.Should().Be("Updated Name");
        }

        // Verify persisted in a third context
        using (var ctx = CreateContext())
        {
            var repo = new FlockRepository(ctx);
            var persisted = await repo.GetByIdWithoutHistoryAsync(flockId);
            persisted.Should().NotBeNull();
            persisted!.Identifier.Should().Be("Updated Name");
        }
    }

    [Fact]
    public async Task UpdateAsync_WhenCompositionChanges_ShouldPersistNewHistoryEntry()
    {
        // Arrange: persist a flock with initial composition (creates 1 history entry)
        var flockId = Guid.Empty;
        using (var ctx = CreateContext())
        {
            var repo = new FlockRepository(ctx);
            var flock = Flock.Create(_tenantId, _coopId, "Test Flock", DateTime.UtcNow.AddDays(-60), 10, 2, 5);
            await repo.AddAsync(flock);
            await ctx.SaveChangesAsync();
            flockId = flock.Id;
        }

        // Act: load without history and update composition in a fresh context
        // This mirrors UpdateFlockCommandHandler's behavior exactly
        using (var ctx = CreateContext())
        {
            var repo = new FlockRepository(ctx);
            var trackedFlock = await repo.GetByIdWithoutHistoryAsync(flockId);
            trackedFlock.Should().NotBeNull();

            // Simulate composition update — adds a new FlockHistory to the tracked collection
            trackedFlock!.UpdateComposition(12, 2, 5, "Manual update");

            await repo.UpdateAsync(trackedFlock);
            await ctx.SaveChangesAsync();
        }

        // Assert: new history entry must be persisted
        using (var ctx = CreateContext())
        {
            var repo = new FlockRepository(ctx);
            var updated = await repo.GetByIdAsync(flockId);
            updated.Should().NotBeNull();
            updated!.CurrentHens.Should().Be(12);

            // Should have initial entry + new composition entry = 2 total
            updated.History.Should().HaveCount(2);
        }
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnFlocksOrderedByCreatedAtDescending()
    {
        // Arrange: persist three flocks with distinct CreatedAt values via reflection
        var flockIds = new List<Guid>();
        var baseTime = DateTime.UtcNow;

        using (var ctx = CreateContext())
        {
            var repo = new FlockRepository(ctx);

            var flock1 = Flock.Create(_tenantId, _coopId, "Alpha", DateTime.UtcNow.AddDays(-90), 5, 1, 0);
            typeof(Flock).GetProperty(nameof(Flock.CreatedAt))!.SetValue(flock1, baseTime.AddDays(-2));

            var flock2 = Flock.Create(_tenantId, _coopId, "Beta", DateTime.UtcNow.AddDays(-60), 6, 1, 0);
            typeof(Flock).GetProperty(nameof(Flock.CreatedAt))!.SetValue(flock2, baseTime.AddDays(-1));

            var flock3 = Flock.Create(_tenantId, _coopId, "Gamma", DateTime.UtcNow.AddDays(-30), 7, 1, 0);
            typeof(Flock).GetProperty(nameof(Flock.CreatedAt))!.SetValue(flock3, baseTime);

            ctx.Flocks.AddRange(flock1, flock2, flock3);
            await ctx.SaveChangesAsync();
            flockIds.AddRange(new[] { flock1.Id, flock2.Id, flock3.Id });
        }

        // Act
        using var readCtx = CreateContext();
        var result = await new FlockRepository(readCtx).GetAllAsync();

        // Assert: newest (Gamma) first, oldest (Alpha) last
        result.Should().HaveCount(3);
        result[0].Identifier.Should().Be("Gamma");
        result[1].Identifier.Should().Be("Beta");
        result[2].Identifier.Should().Be("Alpha");
    }

    [Fact]
    public async Task GetByCoopIdAsync_ShouldReturnFlocksOrderedByCreatedAtDescending()
    {
        // Arrange: persist two flocks for _coopId with distinct CreatedAt values
        var baseTime = DateTime.UtcNow;

        using (var ctx = CreateContext())
        {
            var flock1 = Flock.Create(_tenantId, _coopId, "Older", DateTime.UtcNow.AddDays(-60), 5, 1, 0);
            typeof(Flock).GetProperty(nameof(Flock.CreatedAt))!.SetValue(flock1, baseTime.AddDays(-1));

            var flock2 = Flock.Create(_tenantId, _coopId, "Newer", DateTime.UtcNow.AddDays(-30), 6, 1, 0);
            typeof(Flock).GetProperty(nameof(Flock.CreatedAt))!.SetValue(flock2, baseTime);

            ctx.Flocks.AddRange(flock1, flock2);
            await ctx.SaveChangesAsync();
        }

        // Act
        using var readCtx = CreateContext();
        var result = await new FlockRepository(readCtx).GetByCoopIdAsync(_coopId);

        // Assert: newest first
        result.Should().HaveCount(2);
        result[0].Identifier.Should().Be("Newer");
        result[1].Identifier.Should().Be("Older");
    }

    [Fact]
    public async Task UpdateAsync_WhenEntityIsDetached_ShouldStillSaveChanges()
    {
        // Arrange: persist a flock in a dedicated context
        var flockId = Guid.Empty;
        Flock? detachedFlock = null;
        using (var ctx = CreateContext())
        {
            var repo = new FlockRepository(ctx);
            var flock = Flock.Create(_tenantId, _coopId, "Detached Flock", DateTime.UtcNow.AddDays(-30), 5, 1, 0);
            await repo.AddAsync(flock);
            await ctx.SaveChangesAsync();
            flockId = flock.Id;
            detachedFlock = flock;
        }
        // After the context is disposed, the entity is effectively detached from any context

        // Act: update the entity via a fresh context where it is not yet tracked
        using (var ctx = CreateContext())
        {
            var repo = new FlockRepository(ctx);
            // Verify the entity is not tracked by this context (state = Detached)
            ctx.Entry(detachedFlock!).State.Should().Be(EntityState.Detached);

            detachedFlock.Update("Detached Updated", DateTime.UtcNow.AddDays(-10));
            var result = await repo.UpdateAsync(detachedFlock);
            await ctx.SaveChangesAsync();
            result.Identifier.Should().Be("Detached Updated");
        }

        // Verify persisted
        using (var ctx = CreateContext())
        {
            var repo = new FlockRepository(ctx);
            var persisted = await repo.GetByIdWithoutHistoryAsync(flockId);
            persisted.Should().NotBeNull();
            persisted!.Identifier.Should().Be("Detached Updated");
        }
    }
}
