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
        var flock = Flock.Create(
            _validTenantId,
            _validCoopId,
            ValidIdentifier,
            ValidHatchDate,
            ValidHens,
            ValidRoosters,
            ValidChicks);

        // Assert
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
        var flock = Flock.Create(
            _validTenantId,
            _validCoopId,
            "Hens Only",
            ValidHatchDate,
            20,
            0,
            0);

        // Assert
        flock.Should().NotBeNull();
        flock.CurrentHens.Should().Be(20);
        flock.CurrentRoosters.Should().Be(0);
        flock.CurrentChicks.Should().Be(0);
    }

    [Fact]
    public void Create_WithOnlyRoosters_ShouldSucceed()
    {
        // Arrange & Act
        var flock = Flock.Create(
            _validTenantId,
            _validCoopId,
            "Roosters Only",
            ValidHatchDate,
            0,
            5,
            0);

        // Assert
        flock.Should().NotBeNull();
        flock.CurrentHens.Should().Be(0);
        flock.CurrentRoosters.Should().Be(5);
        flock.CurrentChicks.Should().Be(0);
    }

    [Fact]
    public void Create_WithOnlyChicks_ShouldSucceed()
    {
        // Arrange & Act
        var flock = Flock.Create(
            _validTenantId,
            _validCoopId,
            "Chicks Only",
            ValidHatchDate,
            0,
            0,
            15);

        // Assert
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
        var flock = Flock.Create(
            _validTenantId,
            _validCoopId,
            ValidIdentifier,
            ValidHatchDate,
            ValidHens,
            ValidRoosters,
            ValidChicks,
            notes);

        // Assert
        flock.Should().NotBeNull();
    }

    [Fact]
    public void Create_WithoutNotes_ShouldSucceed()
    {
        // Arrange & Act
        var flock = Flock.Create(
            _validTenantId,
            _validCoopId,
            ValidIdentifier,
            ValidHatchDate,
            ValidHens,
            ValidRoosters,
            ValidChicks);

        // Assert
        flock.Should().NotBeNull();
    }

    [Fact]
    public void Create_ShouldGenerateUniqueIds()
    {
        // Arrange & Act
        var flock1 = Flock.Create(_validTenantId, _validCoopId, "Flock 1", ValidHatchDate, 5, 1, 0);
        var flock2 = Flock.Create(_validTenantId, _validCoopId, "Flock 2", ValidHatchDate, 5, 1, 0);

        // Assert
        flock1.Id.Should().NotBe(flock2.Id);
    }

    #endregion

    #region Validation Tests - Tenant and Coop ID

    [Fact]
    public void Create_WithEmptyTenantId_ShouldThrowArgumentException()
    {
        // Arrange
        var emptyTenantId = Guid.Empty;

        // Act
        var act = () => Flock.Create(
            emptyTenantId,
            _validCoopId,
            ValidIdentifier,
            ValidHatchDate,
            ValidHens,
            ValidRoosters,
            ValidChicks);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Tenant ID cannot be empty.*")
            .And.ParamName.Should().Be("tenantId");
    }

    [Fact]
    public void Create_WithEmptyCoopId_ShouldThrowArgumentException()
    {
        // Arrange
        var emptyCoopId = Guid.Empty;

        // Act
        var act = () => Flock.Create(
            _validTenantId,
            emptyCoopId,
            ValidIdentifier,
            ValidHatchDate,
            ValidHens,
            ValidRoosters,
            ValidChicks);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Coop ID cannot be empty.*")
            .And.ParamName.Should().Be("coopId");
    }

    #endregion

    #region Validation Tests - Identifier

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhitespaceIdentifier_ShouldThrowArgumentException(string? invalidIdentifier)
    {
        // Arrange & Act
        var act = () => Flock.Create(
            _validTenantId,
            _validCoopId,
            invalidIdentifier!,
            ValidHatchDate,
            ValidHens,
            ValidRoosters,
            ValidChicks);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Identifier cannot be empty.*")
            .And.ParamName.Should().Be("identifier");
    }

    [Fact]
    public void Create_WithIdentifierExceeding50Characters_ShouldThrowArgumentException()
    {
        // Arrange
        var longIdentifier = new string('A', 51);

        // Act
        var act = () => Flock.Create(
            _validTenantId,
            _validCoopId,
            longIdentifier,
            ValidHatchDate,
            ValidHens,
            ValidRoosters,
            ValidChicks);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Identifier cannot exceed 50 characters.*")
            .And.ParamName.Should().Be("identifier");
    }

    [Fact]
    public void Create_WithIdentifierExactly50Characters_ShouldSucceed()
    {
        // Arrange
        var identifier = new string('A', 50);

        // Act
        var flock = Flock.Create(
            _validTenantId,
            _validCoopId,
            identifier,
            ValidHatchDate,
            ValidHens,
            ValidRoosters,
            ValidChicks);

        // Assert
        flock.Should().NotBeNull();
        flock.Identifier.Should().Be(identifier);
        flock.Identifier.Length.Should().Be(50);
    }

    #endregion

    #region Validation Tests - Hatch Date

    [Fact]
    public void Create_WithFutureHatchDate_ShouldThrowArgumentException()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.AddDays(1);

        // Act
        var act = () => Flock.Create(
            _validTenantId,
            _validCoopId,
            ValidIdentifier,
            futureDate,
            ValidHens,
            ValidRoosters,
            ValidChicks);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Hatch date cannot be in the future.*")
            .And.ParamName.Should().Be("hatchDate");
    }

    [Fact]
    public void Create_WithTodayHatchDate_ShouldSucceed()
    {
        // Arrange
        var today = DateTime.UtcNow;

        // Act
        var flock = Flock.Create(
            _validTenantId,
            _validCoopId,
            ValidIdentifier,
            today,
            ValidHens,
            ValidRoosters,
            ValidChicks);

        // Assert
        flock.Should().NotBeNull();
        flock.HatchDate.Should().BeCloseTo(today, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_WithPastHatchDate_ShouldSucceed()
    {
        // Arrange
        var pastDate = DateTime.UtcNow.AddYears(-1);

        // Act
        var flock = Flock.Create(
            _validTenantId,
            _validCoopId,
            ValidIdentifier,
            pastDate,
            ValidHens,
            ValidRoosters,
            ValidChicks);

        // Assert
        flock.Should().NotBeNull();
        flock.HatchDate.Should().Be(pastDate);
    }

    #endregion

    #region Validation Tests - Counts

    [Fact]
    public void Create_WithNegativeHensCount_ShouldThrowArgumentException()
    {
        // Arrange & Act
        var act = () => Flock.Create(
            _validTenantId,
            _validCoopId,
            ValidIdentifier,
            ValidHatchDate,
            -1,
            ValidRoosters,
            ValidChicks);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Initial hens count cannot be negative.*")
            .And.ParamName.Should().Be("initialHens");
    }

    [Fact]
    public void Create_WithNegativeRoostersCount_ShouldThrowArgumentException()
    {
        // Arrange & Act
        var act = () => Flock.Create(
            _validTenantId,
            _validCoopId,
            ValidIdentifier,
            ValidHatchDate,
            ValidHens,
            -1,
            ValidChicks);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Initial roosters count cannot be negative.*")
            .And.ParamName.Should().Be("initialRoosters");
    }

    [Fact]
    public void Create_WithNegativeChicksCount_ShouldThrowArgumentException()
    {
        // Arrange & Act
        var act = () => Flock.Create(
            _validTenantId,
            _validCoopId,
            ValidIdentifier,
            ValidHatchDate,
            ValidHens,
            ValidRoosters,
            -1);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Initial chicks count cannot be negative.*")
            .And.ParamName.Should().Be("initialChicks");
    }

    [Fact]
    public void Create_WithAllZeroCounts_ShouldThrowArgumentException()
    {
        // Arrange & Act
        var act = () => Flock.Create(
            _validTenantId,
            _validCoopId,
            ValidIdentifier,
            ValidHatchDate,
            0,
            0,
            0);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("At least one animal type must have a count greater than 0.*")
            .And.ParamName.Should().Be("initialHens");
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
            ValidChicks);

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
            ValidChicks);

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
            ValidChicks);
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
            ValidChicks);

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
            ValidChicks);

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
            notes);

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
            ValidChicks);

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
            ValidChicks);

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
            ValidChicks);
        var originalCreatedAt = flock.CreatedAt;
        var originalUpdatedAt = flock.UpdatedAt;
        Thread.Sleep(10);

        var newIdentifier = "Updated Flock";
        var newHatchDate = DateTime.UtcNow.AddDays(-60);

        // Act
        flock.Update(newIdentifier, newHatchDate);

        // Assert
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
            ValidChicks);

        // Act
        flock.Update("New Identifier", DateTime.UtcNow.AddDays(-45));

        // Assert
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
            ValidChicks);
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
            ValidChicks);
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
            ValidChicks);

        // Act
        flock.UpdateComposition(15, 3, 2, "Purchase", "Bought more chickens");

        // Assert
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
            ValidChicks);

        // Act
        flock.UpdateComposition(15, 3, 2, "Purchase", "Bought more chickens");

        // Assert
        flock.CurrentHens.Should().Be(15);
        flock.CurrentRoosters.Should().Be(3);
        flock.CurrentChicks.Should().Be(2);
    }

    #endregion
}
