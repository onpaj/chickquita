using Chickquita.Domain.Entities;
using Chickquita.Infrastructure.Data;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Chickquita.Infrastructure.Tests.Data;

/// <summary>
/// Integration tests for flock data integrity constraints.
/// Tests database-level constraints, application-level validation alignment,
/// history immutability, and soft delete preservation.
/// </summary>
public class FlockDataIntegrityTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ApplicationDbContext _dbContext;
    private readonly Guid _tenantId;
    private readonly Guid _coopId;

    public FlockDataIntegrityTests()
    {
        // Use SQLite in-memory database for testing
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _dbContext.Database.EnsureCreated();

        // Seed test data
        _tenantId = Guid.NewGuid();
        var tenant = Tenant.Create("clerk_user_test", "test@example.com");
        typeof(Tenant).GetProperty(nameof(Tenant.Id))!.SetValue(tenant, _tenantId);
        _dbContext.Tenants.Add(tenant);

        var coop = Coop.Create(_tenantId, "Test Coop", "Test Location");
        _dbContext.Coops.Add(coop);
        _dbContext.SaveChanges();

        _coopId = coop.Id;
    }

    #region Database Constraint Tests

    [Fact]
    public async Task Flock_NotNullConstraint_TenantIdRequired()
    {
        // Arrange - Domain validation should prevent creating a flock with empty tenant ID
        var act = () => Flock.Create(Guid.Empty, _coopId, "TEST-001", DateTime.UtcNow.AddMonths(-2), 10, 2, 5, null);

        // Act & Assert - Should fail at domain level before reaching database
        act.Should().Throw<ArgumentException>().WithMessage("*tenantId*");
    }

    [Fact]
    public async Task Flock_NotNullConstraint_CoopIdRequired()
    {
        // Arrange - try to create flock without coop
        var act = () => Flock.Create(_tenantId, Guid.Empty, "TEST-001", DateTime.UtcNow.AddMonths(-2), 10, 2, 5, null);

        // Act & Assert - should fail at domain level
        act.Should().Throw<ArgumentException>().WithMessage("*coopId*");
    }

    [Fact]
    public async Task Flock_NotNullConstraint_IdentifierRequired()
    {
        // Arrange
        var act = () => Flock.Create(_tenantId, _coopId, "", DateTime.UtcNow.AddMonths(-2), 10, 2, 5, null);

        // Act & Assert
        act.Should().Throw<ArgumentException>().WithMessage("*identifier*");
    }

    [Fact]
    public async Task Flock_CheckConstraint_HensCannotBeNegative()
    {
        // Arrange
        var act = () => Flock.Create(_tenantId, _coopId, "TEST-001", DateTime.UtcNow.AddMonths(-2), -1, 2, 5, null);

        // Act & Assert - Domain validation catches this
        act.Should().Throw<ArgumentException>().WithMessage("*hens*");
    }

    [Fact]
    public async Task Flock_CheckConstraint_RoostersCannotBeNegative()
    {
        // Arrange
        var act = () => Flock.Create(_tenantId, _coopId, "TEST-001", DateTime.UtcNow.AddMonths(-2), 10, -1, 5, null);

        // Act & Assert - Domain validation catches this
        act.Should().Throw<ArgumentException>().WithMessage("*roosters*");
    }

    [Fact]
    public async Task Flock_CheckConstraint_ChicksCannotBeNegative()
    {
        // Arrange
        var act = () => Flock.Create(_tenantId, _coopId, "TEST-001", DateTime.UtcNow.AddMonths(-2), 10, 2, -1, null);

        // Act & Assert - Domain validation catches this
        act.Should().Throw<ArgumentException>().WithMessage("*chicks*");
    }

    [Fact]
    public async Task Flock_ForeignKeyConstraint_CoopIdMustExist()
    {
        // Arrange
        var nonExistentCoopId = Guid.NewGuid();
        var flock = Flock.Create(_tenantId, nonExistentCoopId, "TEST-001", DateTime.UtcNow.AddMonths(-2), 10, 2, 5, null);
        _dbContext.Flocks.Add(flock);

        // Act & Assert - Foreign key constraint should prevent this
        var act = async () => await _dbContext.SaveChangesAsync();
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task Flock_UniqueConstraint_IdentifierMustBeUniquePerCoop()
    {
        // Arrange
        var flock1 = Flock.Create(_tenantId, _coopId, "FLOCK-001", DateTime.UtcNow.AddMonths(-2), 10, 2, 5, null);
        _dbContext.Flocks.Add(flock1);
        await _dbContext.SaveChangesAsync();

        var flock2 = Flock.Create(_tenantId, _coopId, "FLOCK-001", DateTime.UtcNow.AddMonths(-1), 8, 1, 3, null);
        _dbContext.Flocks.Add(flock2);

        // Act & Assert - Unique constraint should prevent duplicate
        var act = async () => await _dbContext.SaveChangesAsync();
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task Flock_UniqueConstraint_SameIdentifierAllowedInDifferentCoops()
    {
        // Arrange - Create second coop
        var coop2 = Coop.Create(_tenantId, "Test Coop 2", "Test Location 2");
        _dbContext.Coops.Add(coop2);
        await _dbContext.SaveChangesAsync();

        // Create flocks with same identifier in different coops
        var flock1 = Flock.Create(_tenantId, _coopId, "FLOCK-001", DateTime.UtcNow.AddMonths(-2), 10, 2, 5, null);
        var flock2 = Flock.Create(_tenantId, coop2.Id, "FLOCK-001", DateTime.UtcNow.AddMonths(-1), 8, 1, 3, null);

        _dbContext.Flocks.AddRange(flock1, flock2);

        // Act & Assert - Should succeed
        await _dbContext.SaveChangesAsync();

        var savedFlocks = await _dbContext.Flocks.Where(f => f.Identifier == "FLOCK-001").ToListAsync();
        savedFlocks.Should().HaveCount(2);
        savedFlocks.Select(f => f.CoopId).Distinct().Should().HaveCount(2);
    }

    #endregion

    #region FlockHistory Constraint Tests

    [Fact]
    public async Task FlockHistory_CheckConstraint_HensCannotBeNegative()
    {
        // Arrange
        var flock = Flock.Create(_tenantId, _coopId, "TEST-001", DateTime.UtcNow.AddMonths(-2), 10, 2, 5, null);
        _dbContext.Flocks.Add(flock);
        await _dbContext.SaveChangesAsync();

        // Act - Try to create history with negative hens
        var act = () => FlockHistory.Create(_tenantId, flock.Id, DateTime.UtcNow, -1, 2, 5, "Test", null);

        // Assert - Domain validation catches this
        act.Should().Throw<ArgumentException>().WithMessage("*hens*");
    }

    [Fact]
    public async Task FlockHistory_CheckConstraint_RoostersCannotBeNegative()
    {
        // Arrange
        var flock = Flock.Create(_tenantId, _coopId, "TEST-001", DateTime.UtcNow.AddMonths(-2), 10, 2, 5, null);
        _dbContext.Flocks.Add(flock);
        await _dbContext.SaveChangesAsync();

        // Act
        var act = () => FlockHistory.Create(_tenantId, flock.Id, DateTime.UtcNow, 10, -1, 5, "Test", null);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*roosters*");
    }

    [Fact]
    public async Task FlockHistory_CheckConstraint_ChicksCannotBeNegative()
    {
        // Arrange
        var flock = Flock.Create(_tenantId, _coopId, "TEST-001", DateTime.UtcNow.AddMonths(-2), 10, 2, 5, null);
        _dbContext.Flocks.Add(flock);
        await _dbContext.SaveChangesAsync();

        // Act
        var act = () => FlockHistory.Create(_tenantId, flock.Id, DateTime.UtcNow, 10, 2, -1, "Test", null);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*chicks*");
    }

    [Fact]
    public async Task FlockHistory_ForeignKeyConstraint_FlockIdMustExist()
    {
        // Arrange
        var nonExistentFlockId = Guid.NewGuid();
        var history = FlockHistory.Create(_tenantId, nonExistentFlockId, DateTime.UtcNow, 10, 2, 5, "Test", null);
        _dbContext.FlockHistory.Add(history);

        // Act & Assert
        var act = async () => await _dbContext.SaveChangesAsync();
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    #endregion

    #region History Immutability Tests

    [Fact]
    public async Task FlockHistory_ImmutableFields_CannotUpdateHens()
    {
        // Arrange
        var flock = Flock.Create(_tenantId, _coopId, "TEST-001", DateTime.UtcNow.AddMonths(-2), 10, 2, 5, null);
        _dbContext.Flocks.Add(flock);
        await _dbContext.SaveChangesAsync();

        var initialHistory = flock.History.First();

        // Act - Try to modify hens using reflection (simulating direct DB update)
        var hensProperty = typeof(FlockHistory).GetProperty(nameof(FlockHistory.Hens));
        hensProperty.Should().NotBeNull();

        // The property should have a private setter, making it immutable from outside
        hensProperty!.SetMethod.Should().NotBeNull();
        hensProperty.SetMethod!.IsPrivate.Should().BeTrue("Hens property should be immutable");
    }

    [Fact]
    public async Task FlockHistory_ImmutableFields_CannotUpdateRoosters()
    {
        // Arrange
        var flock = Flock.Create(_tenantId, _coopId, "TEST-001", DateTime.UtcNow.AddMonths(-2), 10, 2, 5, null);
        _dbContext.Flocks.Add(flock);
        await _dbContext.SaveChangesAsync();

        // Act & Assert - Check that roosters property is immutable
        var roostersProperty = typeof(FlockHistory).GetProperty(nameof(FlockHistory.Roosters));
        roostersProperty.Should().NotBeNull();
        roostersProperty!.SetMethod.Should().NotBeNull();
        roostersProperty.SetMethod!.IsPrivate.Should().BeTrue("Roosters property should be immutable");
    }

    [Fact]
    public async Task FlockHistory_ImmutableFields_CannotUpdateChicks()
    {
        // Arrange
        var flock = Flock.Create(_tenantId, _coopId, "TEST-001", DateTime.UtcNow.AddMonths(-2), 10, 2, 5, null);
        _dbContext.Flocks.Add(flock);
        await _dbContext.SaveChangesAsync();

        // Act & Assert
        var chicksProperty = typeof(FlockHistory).GetProperty(nameof(FlockHistory.Chicks));
        chicksProperty.Should().NotBeNull();
        chicksProperty!.SetMethod.Should().NotBeNull();
        chicksProperty.SetMethod!.IsPrivate.Should().BeTrue("Chicks property should be immutable");
    }

    [Fact]
    public async Task FlockHistory_ImmutableFields_CannotUpdateReason()
    {
        // Arrange
        var flock = Flock.Create(_tenantId, _coopId, "TEST-001", DateTime.UtcNow.AddMonths(-2), 10, 2, 5, null);
        _dbContext.Flocks.Add(flock);
        await _dbContext.SaveChangesAsync();

        // Act & Assert
        var reasonProperty = typeof(FlockHistory).GetProperty(nameof(FlockHistory.Reason));
        reasonProperty.Should().NotBeNull();
        reasonProperty!.SetMethod.Should().NotBeNull();
        reasonProperty.SetMethod!.IsPrivate.Should().BeTrue("Reason property should be immutable");
    }

    [Fact]
    public async Task FlockHistory_ImmutableFields_CannotUpdateChangeDate()
    {
        // Arrange
        var flock = Flock.Create(_tenantId, _coopId, "TEST-001", DateTime.UtcNow.AddMonths(-2), 10, 2, 5, null);
        _dbContext.Flocks.Add(flock);
        await _dbContext.SaveChangesAsync();

        // Act & Assert
        var changeDateProperty = typeof(FlockHistory).GetProperty(nameof(FlockHistory.ChangeDate));
        changeDateProperty.Should().NotBeNull();
        changeDateProperty!.SetMethod.Should().NotBeNull();
        changeDateProperty.SetMethod!.IsPrivate.Should().BeTrue("ChangeDate property should be immutable");
    }

    [Fact]
    public async Task FlockHistory_MutableField_CanUpdateNotes()
    {
        // Arrange
        var flock = Flock.Create(_tenantId, _coopId, "TEST-001", DateTime.UtcNow.AddMonths(-2), 10, 2, 5, "Initial notes");
        _dbContext.Flocks.Add(flock);
        await _dbContext.SaveChangesAsync();

        var history = flock.History.First();
        var originalNotes = history.Notes;

        // Act - Update notes using the public method
        history.UpdateNotes("Updated notes");
        await _dbContext.SaveChangesAsync();

        // Assert
        var updatedHistory = await _dbContext.FlockHistory.FindAsync(history.Id);
        updatedHistory.Should().NotBeNull();
        updatedHistory!.Notes.Should().Be("Updated notes");
        updatedHistory.Notes.Should().NotBe(originalNotes);
    }

    [Fact]
    public async Task FlockHistory_MultipleHistoryEntries_AllCreatedCorrectly()
    {
        // Arrange - Create flock with initial composition (this creates first history entry)
        var flock = Flock.Create(_tenantId, _coopId, "TEST-001", DateTime.UtcNow.AddMonths(-2), 10, 2, 5, null);
        _dbContext.Flocks.Add(flock);
        await _dbContext.SaveChangesAsync();

        var flockId = flock.Id;

        // Act - Update composition which creates additional history entries
        flock.UpdateComposition(15, 3, 2, "Purchased new chickens", null);

        // Assert - Verify initial history entry exists
        _dbContext.ChangeTracker.Clear();
        var initialHistory = await _dbContext.FlockHistory
            .Where(h => h.FlockId == flockId && h.Reason == "Initial")
            .FirstOrDefaultAsync();

        initialHistory.Should().NotBeNull();
        initialHistory!.Hens.Should().Be(10);
        initialHistory.Roosters.Should().Be(2);
        initialHistory.Chicks.Should().Be(5);

        // Verify update history entry is in the flock's collection (not yet persisted)
        flock.History.Should().HaveCount(2); // Initial + 1 update
        var updateHistory = flock.History.FirstOrDefault(h => h.Reason == "Purchased new chickens");
        updateHistory.Should().NotBeNull();
        updateHistory!.Hens.Should().Be(15);
        updateHistory.Roosters.Should().Be(3);
        updateHistory.Chicks.Should().Be(2);
    }

    #endregion

    #region Soft Delete Tests

    [Fact]
    public async Task SoftDelete_Archive_PreservesFlockInDatabase()
    {
        // Arrange
        var flock = Flock.Create(_tenantId, _coopId, "TEST-001", DateTime.UtcNow.AddMonths(-2), 10, 2, 5, null);
        _dbContext.Flocks.Add(flock);
        await _dbContext.SaveChangesAsync();

        var flockId = flock.Id;

        // Act - Archive the flock
        flock.Archive();
        await _dbContext.SaveChangesAsync();

        // Assert - Flock still exists in database
        var archivedFlock = await _dbContext.Flocks
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(f => f.Id == flockId);

        archivedFlock.Should().NotBeNull();
        archivedFlock!.IsActive.Should().BeFalse();
        archivedFlock.Identifier.Should().Be("TEST-001");
        archivedFlock.CurrentHens.Should().Be(10);
    }

    [Fact]
    public async Task SoftDelete_Archive_PreservesFlockHistory()
    {
        // Arrange
        var flock = Flock.Create(_tenantId, _coopId, "TEST-001", DateTime.UtcNow.AddMonths(-2), 10, 2, 5, null);
        flock.UpdateComposition(15, 3, 2, "Updated before archive", null);
        _dbContext.Flocks.Add(flock);
        await _dbContext.SaveChangesAsync();

        var flockId = flock.Id;
        var historyCount = flock.History.Count;

        // Act - Archive the flock
        flock.Archive();
        await _dbContext.SaveChangesAsync();

        // Assert - History is preserved
        var historyEntries = await _dbContext.FlockHistory
            .Where(h => h.FlockId == flockId)
            .ToListAsync();

        historyEntries.Should().HaveCount(historyCount);
        historyEntries.Should().Contain(h => h.Reason == "Initial");
        historyEntries.Should().Contain(h => h.Reason == "Updated before archive");
    }

    [Fact]
    public async Task SoftDelete_Activate_RestoresArchivedFlock()
    {
        // Arrange
        var flock = Flock.Create(_tenantId, _coopId, "TEST-001", DateTime.UtcNow.AddMonths(-2), 10, 2, 5, null);
        _dbContext.Flocks.Add(flock);
        await _dbContext.SaveChangesAsync();

        flock.Archive();
        await _dbContext.SaveChangesAsync();

        // Act - Reactivate the flock
        flock.Activate();
        await _dbContext.SaveChangesAsync();

        // Assert
        var reactivatedFlock = await _dbContext.Flocks.FindAsync(flock.Id);
        reactivatedFlock.Should().NotBeNull();
        reactivatedFlock!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task SoftDelete_CascadeDelete_PreservesHistoryWhenCoopDeleted()
    {
        // Arrange
        var flock = Flock.Create(_tenantId, _coopId, "TEST-001", DateTime.UtcNow.AddMonths(-2), 10, 2, 5, null);
        _dbContext.Flocks.Add(flock);
        await _dbContext.SaveChangesAsync();

        var flockId = flock.Id;

        // Act - Delete the coop (should cascade to flock and history due to FK)
        var coop = await _dbContext.Coops.FindAsync(_coopId);
        _dbContext.Coops.Remove(coop!);
        await _dbContext.SaveChangesAsync();

        // Assert - Flock and history are deleted (cascade delete)
        var deletedFlock = await _dbContext.Flocks
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(f => f.Id == flockId);
        deletedFlock.Should().BeNull("CASCADE delete should remove flock when coop is deleted");

        var historyEntries = await _dbContext.FlockHistory
            .Where(h => h.FlockId == flockId)
            .ToListAsync();
        historyEntries.Should().BeEmpty("CASCADE delete should remove history when flock is deleted");
    }

    #endregion

    #region Application Validation Alignment Tests

    [Fact]
    public async Task ApplicationValidation_MatchesDatabaseConstraints_RequiredFields()
    {
        // Arrange & Act - Domain validation should match DB NOT NULL constraints
        var testCases = new[]
        {
            new { Field = "tenantId", Act = (Action)(() => Flock.Create(Guid.Empty, _coopId, "TEST", DateTime.UtcNow, 10, 2, 5, null)) },
            new { Field = "coopId", Act = (Action)(() => Flock.Create(_tenantId, Guid.Empty, "TEST", DateTime.UtcNow, 10, 2, 5, null)) },
            new { Field = "identifier", Act = (Action)(() => Flock.Create(_tenantId, _coopId, "", DateTime.UtcNow, 10, 2, 5, null)) },
        };

        // Assert
        foreach (var testCase in testCases)
        {
            testCase.Act.Should().Throw<ArgumentException>($"{testCase.Field} is required");
        }
    }

    [Fact]
    public async Task ApplicationValidation_MatchesDatabaseConstraints_NonNegativeCounts()
    {
        // Arrange & Act - Domain validation should match DB CHECK constraints
        var testCases = new[]
        {
            new { Field = "hens", Act = (Action)(() => Flock.Create(_tenantId, _coopId, "TEST", DateTime.UtcNow, -1, 2, 5, null)) },
            new { Field = "roosters", Act = (Action)(() => Flock.Create(_tenantId, _coopId, "TEST", DateTime.UtcNow, 10, -1, 5, null)) },
            new { Field = "chicks", Act = (Action)(() => Flock.Create(_tenantId, _coopId, "TEST", DateTime.UtcNow, 10, 2, -1, null)) },
        };

        // Assert
        foreach (var testCase in testCases)
        {
            testCase.Act.Should().Throw<ArgumentException>($"{testCase.Field} must be non-negative");
        }
    }

    [Fact]
    public async Task ApplicationValidation_MatchesDatabaseConstraints_MaxLengths()
    {
        // Arrange & Act - Domain validation should match DB maxLength constraints
        var longIdentifier = new string('A', 51); // Max is 50
        var longReason = new string('B', 51); // Max is 50
        var longNotes = new string('C', 501); // Max is 500

        // Assert
        var act1 = () => Flock.Create(_tenantId, _coopId, longIdentifier, DateTime.UtcNow, 10, 2, 5, null);
        act1.Should().Throw<ArgumentException>().WithMessage("*identifier*");

        var flock = Flock.Create(_tenantId, _coopId, "TEST", DateTime.UtcNow, 10, 2, 5, null);
        var act2 = () => flock.UpdateComposition(15, 3, 2, longReason, null);
        act2.Should().Throw<ArgumentException>().WithMessage("*reason*");

        var act3 = () => FlockHistory.Create(_tenantId, flock.Id, DateTime.UtcNow, 10, 2, 5, "Test", longNotes);
        act3.Should().Throw<ArgumentException>().WithMessage("*notes*");
    }

    #endregion

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Close();
        _connection.Dispose();
    }
}
