using Chickquita.Domain.Common;
using Chickquita.Domain.Entities;
using FluentAssertions;

namespace Chickquita.Domain.Tests.Entities;

/// <summary>
/// Unit tests for the Flock domain entity.
/// Tests focus on domain logic, invariants, validation rules, and automatic history creation.
/// </summary>
public class FlockTests
{
    private readonly Guid _validTenantId = Guid.NewGuid();
    private readonly Guid _validCoopId = Guid.NewGuid();
    private const string ValidIdentifier = "Spring 2024";
    private static readonly DateTime ValidHatchDate = DateTime.UtcNow.AddDays(-30);
    private const int ValidHens = 10;
    private const int ValidRoosters = 2;
    private const int ValidChicks = 5;

    #region Create Tests

    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        // Arrange & Act
        var result = Flock.Create(
            _validTenantId,
            _validCoopId,
            ValidIdentifier,
            ValidHatchDate,
            ValidHens,
            ValidRoosters,
            ValidChicks);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var flock = result.Value;
        flock.Should().NotBeNull();
        flock.Id.Should().NotBeEmpty();
        flock.TenantId.Should().Be(_validTenantId);
        flock.CoopId.Should().Be(_validCoopId);
        flock.Identifier.Should().Be(ValidIdentifier);
        flock.HatchDate.Should().Be(ValidHatchDate);
        flock.CurrentHens.Should().Be(ValidHens);
        flock.CurrentRoosters.Should().Be(ValidRoosters);
        flock.CurrentChicks.Should().Be(ValidChicks);
        flock.IsActive.Should().BeTrue();
        flock.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        flock.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        flock.CreatedAt.Should().Be(flock.UpdatedAt);
    }

    [Fact]
    public void Create_WithOnlyHens_ShouldSucceed()
    {
        // Arrange & Act
        var result = Flock.Create(
            _validTenantId,
            _validCoopId,
            "Hens Only",
            ValidHatchDate,
            20,
            0,
            0);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var flock = result.Value;
        flock.Should().NotBeNull();
        flock.CurrentHens.Should().Be(20);
        flock.CurrentRoosters.Should().Be(0);
        flock.CurrentChicks.Should().Be(0);
    }

    [Fact]
    public void Create_WithOnlyRoosters_ShouldSucceed()
    {
        // Arrange & Act
        var result = Flock.Create(
            _validTenantId,
            _validCoopId,
            "Roosters Only",
            ValidHatchDate,
            0,
            5,
            0);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var flock = result.Value;
        flock.Should().NotBeNull();
        flock.CurrentHens.Should().Be(0);
        flock.CurrentRoosters.Should().Be(5);
        flock.CurrentChicks.Should().Be(0);
    }

    [Fact]
    public void Create_WithOnlyChicks_ShouldSucceed()
    {
        // Arrange & Act
        var result = Flock.Create(
            _validTenantId,
            _validCoopId,
            "Chicks Only",
            ValidHatchDate,
            0,
            0,
            15);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var flock = result.Value;
        flock.Should().NotBeNull();
        flock.CurrentHens.Should().Be(0);
        flock.CurrentRoosters.Should().Be(0);
        flock.CurrentChicks.Should().Be(15);
    }

    [Fact]
    public void Create_WithNotes_ShouldSucceed()
    {
        // Arrange
        var notes = "First batch of chickens";

        // Act
        var result = Flock.Create(
            _validTenantId,
            _validCoopId,
            ValidIdentifier,
            ValidHatchDate,
            ValidHens,
            ValidRoosters,
            ValidChicks,
            notes);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public void Create_WithoutNotes_ShouldSucceed()
    {
        // Arrange & Act
        var result = Flock.Create(
            _validTenantId,
            _validCoopId,
            ValidIdentifier,
            ValidHatchDate,
            ValidHens,
            ValidRoosters,
            ValidChicks);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public void Create_ShouldGenerateUniqueIds()
    {
        // Arrange & Act
        var flock1 = Flock.Create(_validTenantId, _validCoopId, "Flock 1", ValidHatchDate, 5, 1, 0).Value;
        var flock2 = Flock.Create(_validTenantId, _validCoopId, "Flock 2", ValidHatchDate, 5, 1, 0).Value;

        // Assert
        flock1.Id.Should().NotBe(flock2.Id);
    }

    #endregion

    #region Validation Tests - Tenant and Coop ID

    [Fact]
    public void Create_WithEmptyTenantId_ShouldReturnFailure()
    {
        // Arrange
        var emptyTenantId = Guid.Empty;

        // Act
        var result = Flock.Create(
            emptyTenantId,
            _validCoopId,
            ValidIdentifier,
            ValidHatchDate,
            ValidHens,
            ValidRoosters,
            ValidChicks);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Tenant ID cannot be empty");
    }

    [Fact]
    public void Create_WithEmptyCoopId_ShouldReturnFailure()
    {
        // Arrange
        var emptyCoopId = Guid.Empty;

        // Act
        var result = Flock.Create(
            _validTenantId,
            emptyCoopId,
            ValidIdentifier,
            ValidHatchDate,
            ValidHens,
            ValidRoosters,
            ValidChicks);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Coop ID cannot be empty");
    }

    #endregion

    #region Validation Tests - Identifier

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhitespaceIdentifier_ShouldReturnFailure(string? invalidIdentifier)
    {
        // Arrange & Act
        var result = Flock.Create(
            _validTenantId,
            _validCoopId,
            invalidIdentifier!,
            ValidHatchDate,
            ValidHens,
            ValidRoosters,
            ValidChicks);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Identifier cannot be empty");
    }

    [Fact]
    public void Create_WithIdentifierExceeding50Characters_ShouldReturnFailure()
    {
        // Arrange
        var longIdentifier = new string('A', 51);

        // Act
        var result = Flock.Create(
            _validTenantId,
            _validCoopId,
            longIdentifier,
            ValidHatchDate,
            ValidHens,
            ValidRoosters,
            ValidChicks);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Identifier cannot exceed 50 characters");
    }

    [Fact]
    public void Create_WithIdentifierExactly50Characters_ShouldSucceed()
    {
        // Arrange
        var identifier = new string('A', 50);

        // Act
        var result = Flock.Create(
            _validTenantId,
            _validCoopId,
            identifier,
            ValidHatchDate,
            ValidHens,
            ValidRoosters,
            ValidChicks);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var flock = result.Value;
        flock.Should().NotBeNull();
        flock.Identifier.Should().Be(identifier);
        flock.Identifier.Length.Should().Be(50);
    }

    #endregion

    #region Validation Tests - Hatch Date

    [Fact]
    public void Create_WithFutureHatchDate_ShouldReturnFailure()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.AddDays(1);

        // Act
        var result = Flock.Create(
            _validTenantId,
            _validCoopId,
            ValidIdentifier,
            futureDate,
            ValidHens,
            ValidRoosters,
            ValidChicks);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Hatch date cannot be in the future");
    }

    [Fact]
    public void Create_WithTodayHatchDate_ShouldSucceed()
    {
        // Arrange
        var today = DateTime.UtcNow;

        // Act
        var result = Flock.Create(
            _validTenantId,
            _validCoopId,
            ValidIdentifier,
            today,
            ValidHens,
            ValidRoosters,
            ValidChicks);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var flock = result.Value;
        flock.Should().NotBeNull();
        flock.HatchDate.Should().BeCloseTo(today, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_WithPastHatchDate_ShouldSucceed()
    {
        // Arrange
        var pastDate = DateTime.UtcNow.AddYears(-1);

        // Act
        var result = Flock.Create(
            _validTenantId,
            _validCoopId,
            ValidIdentifier,
            pastDate,
            ValidHens,
            ValidRoosters,
            ValidChicks);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var flock = result.Value;
        flock.Should().NotBeNull();
        flock.HatchDate.Should().Be(pastDate);
    }

    #endregion

    #region Validation Tests - Counts

    [Fact]
    public void Create_WithNegativeHensCount_ShouldReturnFailure()
    {
        // Arrange & Act
        var result = Flock.Create(
            _validTenantId,
            _validCoopId,
            ValidIdentifier,
            ValidHatchDate,
            -1,
            ValidRoosters,
            ValidChicks);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Initial hens count cannot be negative");
    }

    [Fact]
    public void Create_WithNegativeRoostersCount_ShouldReturnFailure()
    {
        // Arrange & Act
        var result = Flock.Create(
            _validTenantId,
            _validCoopId,
            ValidIdentifier,
            ValidHatchDate,
            ValidHens,
            -1,
            ValidChicks);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Initial roosters count cannot be negative");
    }

    [Fact]
    public void Create_WithNegativeChicksCount_ShouldReturnFailure()
    {
        // Arrange & Act
        var result = Flock.Create(
            _validTenantId,
            _validCoopId,
            ValidIdentifier,
            ValidHatchDate,
            ValidHens,
            ValidRoosters,
            -1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Initial chicks count cannot be negative");
    }

    [Fact]
    public void Create_WithAllZeroCounts_ShouldReturnFailure()
    {
        // Arrange & Act
        var result = Flock.Create(
            _validTenantId,
            _validCoopId,
            ValidIdentifier,
            ValidHatchDate,
            0,
            0,
            0);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("At least one animal type must have a count greater than 0");
    }

    #endregion

    #region History Auto-Creation Tests

    [Fact]
    public void Create_ShouldAutomaticallyCreateInitialHistoryEntry()
    {
        // Arrange & Act
        var flock = Flock.Create(
            _validTenantId,
            _validCoopId,
            ValidIdentifier,
            ValidHatchDate,
            ValidHens,
            ValidRoosters,
            ValidChicks).Value;

        // Assert
        flock.History.Should().NotBeNull();
        flock.History.Should().HaveCount(1, "initial history entry should be created automatically");
    }

    [Fact]
    public void Create_InitialHistoryEntry_ShouldHaveTypeInitial()
    {
        // Arrange & Act
        var flock = Flock.Create(
            _validTenantId,
            _validCoopId,
            ValidIdentifier,
            ValidHatchDate,
            ValidHens,
            ValidRoosters,
            ValidChicks).Value;

        // Assert
        var initialHistory = flock.History.First();
        initialHistory.Reason.Should().Be("Initial", "initial history entry should have 'Initial' reason");
    }

    [Fact]
    public void Create_InitialHistoryEntry_ShouldHaveCorrectDate()
    {
        // Arrange & Act
        var beforeCreate = DateTime.UtcNow;
        var flock = Flock.Create(
            _validTenantId,
            _validCoopId,
            ValidIdentifier,
            ValidHatchDate,
            ValidHens,
            ValidRoosters,
            ValidChicks).Value;
        var afterCreate = DateTime.UtcNow;

        // Assert
        var initialHistory = flock.History.First();
        initialHistory.ChangeDate.Should().BeOnOrAfter(beforeCreate);
        initialHistory.ChangeDate.Should().BeOnOrBefore(afterCreate);
    }

    [Fact]
    public void Create_InitialHistoryEntry_ShouldHaveCorrectComposition()
    {
        // Arrange & Act
        var flock = Flock.Create(
            _validTenantId,
            _validCoopId,
            ValidIdentifier,
            ValidHatchDate,
            ValidHens,
            ValidRoosters,
            ValidChicks).Value;

        // Assert
        var initialHistory = flock.History.First();
        initialHistory.Hens.Should().Be(ValidHens, "history should match initial hens count");
        initialHistory.Roosters.Should().Be(ValidRoosters, "history should match initial roosters count");
        initialHistory.Chicks.Should().Be(ValidChicks, "history should match initial chicks count");
    }

    [Fact]
    public void Create_InitialHistoryEntry_ShouldBelongToCorrectTenant()
    {
        // Arrange & Act
        var flock = Flock.Create(
            _validTenantId,
            _validCoopId,
            ValidIdentifier,
            ValidHatchDate,
            ValidHens,
            ValidRoosters,
            ValidChicks).Value;

        // Assert
        var initialHistory = flock.History.First();
        initialHistory.TenantId.Should().Be(_validTenantId, "history entry should belong to the same tenant as the flock");
        initialHistory.FlockId.Should().Be(flock.Id, "history entry should reference the correct flock");
    }

    [Fact]
    public void Create_InitialHistoryEntry_WithNotes_ShouldIncludeNotes()
    {
        // Arrange
        var notes = "First batch - very healthy chicks";

        // Act
        var flock = Flock.Create(
            _validTenantId,
            _validCoopId,
            ValidIdentifier,
            ValidHatchDate,
            ValidHens,
            ValidRoosters,
            ValidChicks,
            notes).Value;

        // Assert
        var initialHistory = flock.History.First();
        initialHistory.Notes.Should().Be(notes, "history entry should preserve initial notes");
    }

    [Fact]
    public void Create_InitialHistoryEntry_WithoutNotes_ShouldHaveNullNotes()
    {
        // Arrange & Act
        var flock = Flock.Create(
            _validTenantId,
            _validCoopId,
            ValidIdentifier,
            ValidHatchDate,
            ValidHens,
            ValidRoosters,
            ValidChicks).Value;

        // Assert
        var initialHistory = flock.History.First();
        initialHistory.Notes.Should().BeNull("history entry should have null notes when none provided");
    }

    [Fact]
    public void Create_InitialHistoryEntry_ShouldBeImmutable()
    {
        // Arrange & Act
        var flock = Flock.Create(
            _validTenantId,
            _validCoopId,
            ValidIdentifier,
            ValidHatchDate,
            ValidHens,
            ValidRoosters,
            ValidChicks).Value;

        var initialHistory = flock.History.First();
        var originalHens = initialHistory.Hens;
        var originalRoosters = initialHistory.Roosters;
        var originalChicks = initialHistory.Chicks;
        var originalReason = initialHistory.Reason;

        // Assert - Verify properties are read-only (no public setters)
        initialHistory.Hens.Should().Be(originalHens);
        initialHistory.Roosters.Should().Be(originalRoosters);
        initialHistory.Chicks.Should().Be(originalChicks);
        initialHistory.Reason.Should().Be(originalReason);
    }

    #endregion

    #region Update Tests

    [Fact]
    public void Update_WithValidData_ShouldUpdateIdentifierAndHatchDate()
    {
        // Arrange
        var flock = Flock.Create(
            _validTenantId,
            _validCoopId,
            ValidIdentifier,
            ValidHatchDate,
            ValidHens,
            ValidRoosters,
            ValidChicks).Value;
        var originalCreatedAt = flock.CreatedAt;
        var originalUpdatedAt = flock.UpdatedAt;
        Thread.Sleep(10);

        var newIdentifier = "Updated Flock";
        var newHatchDate = DateTime.UtcNow.AddDays(-60);

        // Act
        var updateResult = flock.Update(newIdentifier, newHatchDate);

        // Assert
        updateResult.IsSuccess.Should().BeTrue();
        flock.Identifier.Should().Be(newIdentifier);
        flock.HatchDate.Should().Be(newHatchDate);
        flock.CreatedAt.Should().Be(originalCreatedAt);
        flock.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void Update_ShouldNotModifyCompositionCounts()
    {
        // Arrange
        var flock = Flock.Create(
            _validTenantId,
            _validCoopId,
            ValidIdentifier,
            ValidHatchDate,
            ValidHens,
            ValidRoosters,
            ValidChicks).Value;

        // Act
        var updateResult = flock.Update("New Identifier", DateTime.UtcNow.AddDays(-45));

        // Assert
        updateResult.IsSuccess.Should().BeTrue();
        flock.CurrentHens.Should().Be(ValidHens);
        flock.CurrentRoosters.Should().Be(ValidRoosters);
        flock.CurrentChicks.Should().Be(ValidChicks);
    }

    #endregion

    #region Archive/Activate Tests

    [Fact]
    public void Archive_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var flock = Flock.Create(
            _validTenantId,
            _validCoopId,
            ValidIdentifier,
            ValidHatchDate,
            ValidHens,
            ValidRoosters,
            ValidChicks).Value;
        var originalUpdatedAt = flock.UpdatedAt;
        Thread.Sleep(10);

        // Act
        flock.Archive();

        // Assert
        flock.IsActive.Should().BeFalse();
        flock.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var flock = Flock.Create(
            _validTenantId,
            _validCoopId,
            ValidIdentifier,
            ValidHatchDate,
            ValidHens,
            ValidRoosters,
            ValidChicks).Value;
        flock.Archive();
        Thread.Sleep(10);
        var originalUpdatedAt = flock.UpdatedAt;

        // Act
        flock.Activate();

        // Assert
        flock.IsActive.Should().BeTrue();
        flock.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    #endregion

    #region UpdateComposition Tests

    [Fact]
    public void UpdateComposition_ShouldCreateNewHistoryEntry()
    {
        // Arrange
        var flock = Flock.Create(
            _validTenantId,
            _validCoopId,
            ValidIdentifier,
            ValidHatchDate,
            ValidHens,
            ValidRoosters,
            ValidChicks).Value;

        // Act
        var updateResult = flock.UpdateComposition(15, 3, 2, "Purchase", "Bought more chickens");

        // Assert
        updateResult.IsSuccess.Should().BeTrue();
        flock.History.Should().HaveCount(2, "should have initial history + new update entry");
    }

    [Fact]
    public void UpdateComposition_ShouldUpdateCurrentCounts()
    {
        // Arrange
        var flock = Flock.Create(
            _validTenantId,
            _validCoopId,
            ValidIdentifier,
            ValidHatchDate,
            ValidHens,
            ValidRoosters,
            ValidChicks).Value;

        // Act
        var updateResult = flock.UpdateComposition(15, 3, 2, "Purchase", "Bought more chickens");

        // Assert
        updateResult.IsSuccess.Should().BeTrue();
        flock.CurrentHens.Should().Be(15);
        flock.CurrentRoosters.Should().Be(3);
        flock.CurrentChicks.Should().Be(2);
    }

    #endregion
}
