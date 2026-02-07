using Chickquita.Domain.Entities;
using FluentAssertions;

namespace Chickquita.Domain.Tests.Entities;

/// <summary>
/// Unit tests for the FlockHistory domain entity.
/// Tests focus on immutability, validation rules, and the UpdateNotes method.
/// </summary>
public class FlockHistoryTests
{
    private readonly Guid _validTenantId = Guid.NewGuid();
    private readonly Guid _validFlockId = Guid.NewGuid();
    private static readonly DateTime ValidChangeDate = DateTime.UtcNow.AddDays(-10);
    private const int ValidHens = 10;
    private const int ValidRoosters = 2;
    private const int ValidChicks = 5;
    private const string ValidReason = "Initial";

    #region Create Tests

    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        // Arrange & Act
        var history = FlockHistory.Create(
            _validTenantId,
            _validFlockId,
            ValidChangeDate,
            ValidHens,
            ValidRoosters,
            ValidChicks,
            ValidReason);

        // Assert
        history.Should().NotBeNull();
        history.Id.Should().NotBeEmpty();
        history.TenantId.Should().Be(_validTenantId);
        history.FlockId.Should().Be(_validFlockId);
        history.ChangeDate.Should().Be(ValidChangeDate);
        history.Hens.Should().Be(ValidHens);
        history.Roosters.Should().Be(ValidRoosters);
        history.Chicks.Should().Be(ValidChicks);
        history.Reason.Should().Be(ValidReason);
        history.Notes.Should().BeNull();
        history.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        history.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        history.CreatedAt.Should().Be(history.UpdatedAt);
    }

    [Fact]
    public void Create_WithNotes_ShouldSucceed()
    {
        // Arrange
        var notes = "First batch of chickens";

        // Act
        var history = FlockHistory.Create(
            _validTenantId,
            _validFlockId,
            ValidChangeDate,
            ValidHens,
            ValidRoosters,
            ValidChicks,
            ValidReason,
            notes);

        // Assert
        history.Should().NotBeNull();
        history.Notes.Should().Be(notes);
    }

    [Fact]
    public void Create_WithoutNotes_ShouldHaveNullNotes()
    {
        // Arrange & Act
        var history = FlockHistory.Create(
            _validTenantId,
            _validFlockId,
            ValidChangeDate,
            ValidHens,
            ValidRoosters,
            ValidChicks,
            ValidReason);

        // Assert
        history.Notes.Should().BeNull();
    }

    [Fact]
    public void Create_WithZeroCounts_ShouldSucceed()
    {
        // Arrange & Act
        var history = FlockHistory.Create(
            _validTenantId,
            _validFlockId,
            ValidChangeDate,
            0,
            0,
            0,
            "All sold");

        // Assert
        history.Should().NotBeNull();
        history.Hens.Should().Be(0);
        history.Roosters.Should().Be(0);
        history.Chicks.Should().Be(0);
    }

    [Fact]
    public void Create_ShouldGenerateUniqueIds()
    {
        // Arrange & Act
        var history1 = FlockHistory.Create(_validTenantId, _validFlockId, ValidChangeDate, 5, 1, 0, "Reason 1");
        var history2 = FlockHistory.Create(_validTenantId, _validFlockId, ValidChangeDate, 5, 1, 0, "Reason 2");

        // Assert
        history1.Id.Should().NotBe(history2.Id);
    }

    #endregion

    #region Validation Tests - Tenant and Flock ID

    [Fact]
    public void Create_WithEmptyTenantId_ShouldThrowArgumentException()
    {
        // Arrange
        var emptyTenantId = Guid.Empty;

        // Act
        var act = () => FlockHistory.Create(
            emptyTenantId,
            _validFlockId,
            ValidChangeDate,
            ValidHens,
            ValidRoosters,
            ValidChicks,
            ValidReason);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Tenant ID cannot be empty.*")
            .And.ParamName.Should().Be("tenantId");
    }

    [Fact]
    public void Create_WithEmptyFlockId_ShouldThrowArgumentException()
    {
        // Arrange
        var emptyFlockId = Guid.Empty;

        // Act
        var act = () => FlockHistory.Create(
            _validTenantId,
            emptyFlockId,
            ValidChangeDate,
            ValidHens,
            ValidRoosters,
            ValidChicks,
            ValidReason);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Flock ID cannot be empty.*")
            .And.ParamName.Should().Be("flockId");
    }

    #endregion

    #region Validation Tests - Counts

    [Fact]
    public void Create_WithNegativeHensCount_ShouldThrowArgumentException()
    {
        // Arrange & Act
        var act = () => FlockHistory.Create(
            _validTenantId,
            _validFlockId,
            ValidChangeDate,
            -1,
            ValidRoosters,
            ValidChicks,
            ValidReason);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Hens count cannot be negative.*")
            .And.ParamName.Should().Be("hens");
    }

    [Fact]
    public void Create_WithNegativeRoostersCount_ShouldThrowArgumentException()
    {
        // Arrange & Act
        var act = () => FlockHistory.Create(
            _validTenantId,
            _validFlockId,
            ValidChangeDate,
            ValidHens,
            -1,
            ValidChicks,
            ValidReason);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Roosters count cannot be negative.*")
            .And.ParamName.Should().Be("roosters");
    }

    [Fact]
    public void Create_WithNegativeChicksCount_ShouldThrowArgumentException()
    {
        // Arrange & Act
        var act = () => FlockHistory.Create(
            _validTenantId,
            _validFlockId,
            ValidChangeDate,
            ValidHens,
            ValidRoosters,
            -1,
            ValidReason);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Chicks count cannot be negative.*")
            .And.ParamName.Should().Be("chicks");
    }

    #endregion

    #region Validation Tests - Reason

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhitespaceReason_ShouldThrowArgumentException(string? invalidReason)
    {
        // Arrange & Act
        var act = () => FlockHistory.Create(
            _validTenantId,
            _validFlockId,
            ValidChangeDate,
            ValidHens,
            ValidRoosters,
            ValidChicks,
            invalidReason!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Reason cannot be empty.*")
            .And.ParamName.Should().Be("reason");
    }

    [Fact]
    public void Create_WithReasonExceeding50Characters_ShouldThrowArgumentException()
    {
        // Arrange
        var longReason = new string('A', 51);

        // Act
        var act = () => FlockHistory.Create(
            _validTenantId,
            _validFlockId,
            ValidChangeDate,
            ValidHens,
            ValidRoosters,
            ValidChicks,
            longReason);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Reason cannot exceed 50 characters.*")
            .And.ParamName.Should().Be("reason");
    }

    [Fact]
    public void Create_WithReasonExactly50Characters_ShouldSucceed()
    {
        // Arrange
        var reason = new string('A', 50);

        // Act
        var history = FlockHistory.Create(
            _validTenantId,
            _validFlockId,
            ValidChangeDate,
            ValidHens,
            ValidRoosters,
            ValidChicks,
            reason);

        // Assert
        history.Should().NotBeNull();
        history.Reason.Should().Be(reason);
        history.Reason.Length.Should().Be(50);
    }

    #endregion

    #region Validation Tests - Notes

    [Fact]
    public void Create_WithNotesExceeding500Characters_ShouldThrowArgumentException()
    {
        // Arrange
        var longNotes = new string('B', 501);

        // Act
        var act = () => FlockHistory.Create(
            _validTenantId,
            _validFlockId,
            ValidChangeDate,
            ValidHens,
            ValidRoosters,
            ValidChicks,
            ValidReason,
            longNotes);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Notes cannot exceed 500 characters.*")
            .And.ParamName.Should().Be("notes");
    }

    [Fact]
    public void Create_WithNotesExactly500Characters_ShouldSucceed()
    {
        // Arrange
        var notes = new string('B', 500);

        // Act
        var history = FlockHistory.Create(
            _validTenantId,
            _validFlockId,
            ValidChangeDate,
            ValidHens,
            ValidRoosters,
            ValidChicks,
            ValidReason,
            notes);

        // Assert
        history.Should().NotBeNull();
        history.Notes.Should().Be(notes);
        history.Notes!.Length.Should().Be(500);
    }

    #endregion

    #region Immutability Tests

    [Fact]
    public void FlockHistory_CompositionFields_ShouldBeImmutable()
    {
        // Arrange
        var history = FlockHistory.Create(
            _validTenantId,
            _validFlockId,
            ValidChangeDate,
            ValidHens,
            ValidRoosters,
            ValidChicks,
            ValidReason);

        var originalHens = history.Hens;
        var originalRoosters = history.Roosters;
        var originalChicks = history.Chicks;

        // Assert - Properties should be read-only (no public setters)
        history.Hens.Should().Be(originalHens, "Hens should be immutable");
        history.Roosters.Should().Be(originalRoosters, "Roosters should be immutable");
        history.Chicks.Should().Be(originalChicks, "Chicks should be immutable");
    }

    [Fact]
    public void FlockHistory_ReasonField_ShouldBeImmutable()
    {
        // Arrange
        var history = FlockHistory.Create(
            _validTenantId,
            _validFlockId,
            ValidChangeDate,
            ValidHens,
            ValidRoosters,
            ValidChicks,
            ValidReason);

        var originalReason = history.Reason;

        // Assert - Reason should be read-only (no public setter)
        history.Reason.Should().Be(originalReason, "Reason should be immutable");
    }

    [Fact]
    public void FlockHistory_ChangeDateField_ShouldBeImmutable()
    {
        // Arrange
        var history = FlockHistory.Create(
            _validTenantId,
            _validFlockId,
            ValidChangeDate,
            ValidHens,
            ValidRoosters,
            ValidChicks,
            ValidReason);

        var originalChangeDate = history.ChangeDate;

        // Assert - ChangeDate should be read-only (no public setter)
        history.ChangeDate.Should().Be(originalChangeDate, "ChangeDate should be immutable");
    }

    [Fact]
    public void FlockHistory_TenantAndFlockIds_ShouldBeImmutable()
    {
        // Arrange
        var history = FlockHistory.Create(
            _validTenantId,
            _validFlockId,
            ValidChangeDate,
            ValidHens,
            ValidRoosters,
            ValidChicks,
            ValidReason);

        var originalTenantId = history.TenantId;
        var originalFlockId = history.FlockId;

        // Assert - IDs should be read-only (no public setters)
        history.TenantId.Should().Be(originalTenantId, "TenantId should be immutable");
        history.FlockId.Should().Be(originalFlockId, "FlockId should be immutable");
    }

    #endregion

    #region UpdateNotes Tests - Only Mutable Field

    [Fact]
    public void UpdateNotes_WithValidNotes_ShouldUpdateNotesField()
    {
        // Arrange
        var history = FlockHistory.Create(
            _validTenantId,
            _validFlockId,
            ValidChangeDate,
            ValidHens,
            ValidRoosters,
            ValidChicks,
            ValidReason);

        var originalUpdatedAt = history.UpdatedAt;
        Thread.Sleep(10);
        var newNotes = "Updated notes";

        // Act
        history.UpdateNotes(newNotes);

        // Assert
        history.Notes.Should().Be(newNotes);
        history.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void UpdateNotes_WithNull_ShouldClearNotes()
    {
        // Arrange
        var history = FlockHistory.Create(
            _validTenantId,
            _validFlockId,
            ValidChangeDate,
            ValidHens,
            ValidRoosters,
            ValidChicks,
            ValidReason,
            "Initial notes");

        // Act
        history.UpdateNotes(null);

        // Assert
        history.Notes.Should().BeNull();
    }

    [Fact]
    public void UpdateNotes_ShouldNotModifyOtherFields()
    {
        // Arrange
        var history = FlockHistory.Create(
            _validTenantId,
            _validFlockId,
            ValidChangeDate,
            ValidHens,
            ValidRoosters,
            ValidChicks,
            ValidReason);

        var originalId = history.Id;
        var originalTenantId = history.TenantId;
        var originalFlockId = history.FlockId;
        var originalChangeDate = history.ChangeDate;
        var originalHens = history.Hens;
        var originalRoosters = history.Roosters;
        var originalChicks = history.Chicks;
        var originalReason = history.Reason;
        var originalCreatedAt = history.CreatedAt;

        // Act
        history.UpdateNotes("New notes");

        // Assert
        history.Id.Should().Be(originalId);
        history.TenantId.Should().Be(originalTenantId);
        history.FlockId.Should().Be(originalFlockId);
        history.ChangeDate.Should().Be(originalChangeDate);
        history.Hens.Should().Be(originalHens);
        history.Roosters.Should().Be(originalRoosters);
        history.Chicks.Should().Be(originalChicks);
        history.Reason.Should().Be(originalReason);
        history.CreatedAt.Should().Be(originalCreatedAt);
    }

    [Fact]
    public void UpdateNotes_WithNotesExceeding500Characters_ShouldThrowArgumentException()
    {
        // Arrange
        var history = FlockHistory.Create(
            _validTenantId,
            _validFlockId,
            ValidChangeDate,
            ValidHens,
            ValidRoosters,
            ValidChicks,
            ValidReason);

        var longNotes = new string('C', 501);

        // Act
        var act = () => history.UpdateNotes(longNotes);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Notes cannot exceed 500 characters.*")
            .And.ParamName.Should().Be("notes");
    }

    [Fact]
    public void UpdateNotes_WithExactly500Characters_ShouldSucceed()
    {
        // Arrange
        var history = FlockHistory.Create(
            _validTenantId,
            _validFlockId,
            ValidChangeDate,
            ValidHens,
            ValidRoosters,
            ValidChicks,
            ValidReason);

        var notes = new string('C', 500);

        // Act
        history.UpdateNotes(notes);

        // Assert
        history.Notes.Should().Be(notes);
        history.Notes!.Length.Should().Be(500);
    }

    [Fact]
    public void UpdateNotes_MultipleTimes_ShouldAlwaysSucceed()
    {
        // Arrange
        var history = FlockHistory.Create(
            _validTenantId,
            _validFlockId,
            ValidChangeDate,
            ValidHens,
            ValidRoosters,
            ValidChicks,
            ValidReason);

        // Act & Assert
        history.UpdateNotes("First update");
        history.Notes.Should().Be("First update");

        history.UpdateNotes("Second update");
        history.Notes.Should().Be("Second update");

        history.UpdateNotes(null);
        history.Notes.Should().BeNull();

        history.UpdateNotes("Third update");
        history.Notes.Should().Be("Third update");
    }

    #endregion

    #region Common Reason Values Tests

    [Theory]
    [InlineData("Initial")]
    [InlineData("Maturation")]
    [InlineData("Purchase")]
    [InlineData("Death")]
    [InlineData("Sale")]
    public void Create_WithCommonReasonValues_ShouldSucceed(string reason)
    {
        // Arrange & Act
        var history = FlockHistory.Create(
            _validTenantId,
            _validFlockId,
            ValidChangeDate,
            ValidHens,
            ValidRoosters,
            ValidChicks,
            reason);

        // Assert
        history.Should().NotBeNull();
        history.Reason.Should().Be(reason);
    }

    #endregion
}
