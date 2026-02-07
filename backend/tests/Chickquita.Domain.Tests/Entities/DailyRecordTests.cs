using Chickquita.Domain.Entities;
using FluentAssertions;

namespace Chickquita.Domain.Tests.Entities;

/// <summary>
/// Unit tests for the DailyRecord domain entity.
/// Tests focus on domain logic, invariants, and validation rules.
/// </summary>
public class DailyRecordTests
{
    private readonly Guid _validTenantId = Guid.NewGuid();
    private readonly Guid _validFlockId = Guid.NewGuid();
    private static readonly DateTime ValidRecordDate = DateTime.UtcNow.AddDays(-1).Date;
    private const int ValidEggCount = 15;
    private const string ValidNotes = "Good collection today";

    #region Create Tests

    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        // Arrange & Act
        var dailyRecord = DailyRecord.Create(
            _validTenantId,
            _validFlockId,
            ValidRecordDate,
            ValidEggCount,
            ValidNotes);

        // Assert
        dailyRecord.Should().NotBeNull();
        dailyRecord.Id.Should().NotBeEmpty();
        dailyRecord.TenantId.Should().Be(_validTenantId);
        dailyRecord.FlockId.Should().Be(_validFlockId);
        dailyRecord.RecordDate.Should().Be(ValidRecordDate);
        dailyRecord.EggCount.Should().Be(ValidEggCount);
        dailyRecord.Notes.Should().Be(ValidNotes);
        dailyRecord.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        dailyRecord.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        dailyRecord.CreatedAt.Should().Be(dailyRecord.UpdatedAt);
    }

    [Fact]
    public void Create_WithoutNotes_ShouldSucceed()
    {
        // Arrange & Act
        var dailyRecord = DailyRecord.Create(
            _validTenantId,
            _validFlockId,
            ValidRecordDate,
            ValidEggCount);

        // Assert
        dailyRecord.Should().NotBeNull();
        dailyRecord.Notes.Should().BeNull();
    }

    [Fact]
    public void Create_WithZeroEggCount_ShouldSucceed()
    {
        // Arrange & Act
        var dailyRecord = DailyRecord.Create(
            _validTenantId,
            _validFlockId,
            ValidRecordDate,
            0);

        // Assert
        dailyRecord.Should().NotBeNull();
        dailyRecord.EggCount.Should().Be(0);
    }

    [Fact]
    public void Create_ShouldGenerateUniqueIds()
    {
        // Arrange & Act
        var record1 = DailyRecord.Create(_validTenantId, _validFlockId, ValidRecordDate, 10);
        var record2 = DailyRecord.Create(_validTenantId, _validFlockId, ValidRecordDate.AddDays(-1), 12);

        // Assert
        record1.Id.Should().NotBe(record2.Id);
    }

    [Fact]
    public void Create_ShouldNormalizeDateToMidnight()
    {
        // Arrange
        var dateWithTime = DateTime.UtcNow.AddDays(-1).AddHours(14).AddMinutes(30);

        // Act
        var dailyRecord = DailyRecord.Create(
            _validTenantId,
            _validFlockId,
            dateWithTime,
            ValidEggCount);

        // Assert
        dailyRecord.RecordDate.Hour.Should().Be(0);
        dailyRecord.RecordDate.Minute.Should().Be(0);
        dailyRecord.RecordDate.Second.Should().Be(0);
        dailyRecord.RecordDate.Millisecond.Should().Be(0);
    }

    #endregion

    #region Validation Tests - Tenant and Flock ID

    [Fact]
    public void Create_WithEmptyTenantId_ShouldThrowArgumentException()
    {
        // Arrange
        var emptyTenantId = Guid.Empty;

        // Act
        var act = () => DailyRecord.Create(
            emptyTenantId,
            _validFlockId,
            ValidRecordDate,
            ValidEggCount);

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
        var act = () => DailyRecord.Create(
            _validTenantId,
            emptyFlockId,
            ValidRecordDate,
            ValidEggCount);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Flock ID cannot be empty.*")
            .And.ParamName.Should().Be("flockId");
    }

    #endregion

    #region Validation Tests - Record Date

    [Fact]
    public void Create_WithFutureRecordDate_ShouldThrowArgumentException()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.Date.AddDays(1);

        // Act
        var act = () => DailyRecord.Create(
            _validTenantId,
            _validFlockId,
            futureDate,
            ValidEggCount);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Record date cannot be in the future.*")
            .And.ParamName.Should().Be("recordDate");
    }

    [Fact]
    public void Create_WithTodayRecordDate_ShouldSucceed()
    {
        // Arrange
        var today = DateTime.UtcNow.Date;

        // Act
        var dailyRecord = DailyRecord.Create(
            _validTenantId,
            _validFlockId,
            today,
            ValidEggCount);

        // Assert
        dailyRecord.Should().NotBeNull();
        dailyRecord.RecordDate.Should().Be(today);
    }

    [Fact]
    public void Create_WithPastRecordDate_ShouldSucceed()
    {
        // Arrange
        var pastDate = DateTime.UtcNow.Date.AddYears(-1);

        // Act
        var dailyRecord = DailyRecord.Create(
            _validTenantId,
            _validFlockId,
            pastDate,
            ValidEggCount);

        // Assert
        dailyRecord.Should().NotBeNull();
        dailyRecord.RecordDate.Should().Be(pastDate);
    }

    [Fact]
    public void Create_WithLocalTime_ShouldConvertToUtc()
    {
        // Arrange
        var localDate = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Local);

        // Act
        var dailyRecord = DailyRecord.Create(
            _validTenantId,
            _validFlockId,
            localDate,
            ValidEggCount);

        // Assert
        dailyRecord.RecordDate.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void Create_WithUnspecifiedKind_ShouldConvertToUtc()
    {
        // Arrange
        var unspecifiedDate = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Unspecified);

        // Act
        var dailyRecord = DailyRecord.Create(
            _validTenantId,
            _validFlockId,
            unspecifiedDate,
            ValidEggCount);

        // Assert
        dailyRecord.RecordDate.Kind.Should().Be(DateTimeKind.Utc);
    }

    #endregion

    #region Validation Tests - Egg Count

    [Fact]
    public void Create_WithNegativeEggCount_ShouldThrowArgumentException()
    {
        // Arrange & Act
        var act = () => DailyRecord.Create(
            _validTenantId,
            _validFlockId,
            ValidRecordDate,
            -1);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Egg count cannot be negative.*")
            .And.ParamName.Should().Be("eggCount");
    }

    [Fact]
    public void Create_WithLargeEggCount_ShouldSucceed()
    {
        // Arrange & Act
        var dailyRecord = DailyRecord.Create(
            _validTenantId,
            _validFlockId,
            ValidRecordDate,
            1000);

        // Assert
        dailyRecord.Should().NotBeNull();
        dailyRecord.EggCount.Should().Be(1000);
    }

    #endregion

    #region Validation Tests - Notes

    [Fact]
    public void Create_WithNotesExceeding500Characters_ShouldThrowArgumentException()
    {
        // Arrange
        var longNotes = new string('A', 501);

        // Act
        var act = () => DailyRecord.Create(
            _validTenantId,
            _validFlockId,
            ValidRecordDate,
            ValidEggCount,
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
        var notes = new string('A', 500);

        // Act
        var dailyRecord = DailyRecord.Create(
            _validTenantId,
            _validFlockId,
            ValidRecordDate,
            ValidEggCount,
            notes);

        // Assert
        dailyRecord.Should().NotBeNull();
        dailyRecord.Notes.Should().Be(notes);
        dailyRecord.Notes!.Length.Should().Be(500);
    }

    [Fact]
    public void Create_WithEmptyStringNotes_ShouldSucceed()
    {
        // Arrange & Act
        var dailyRecord = DailyRecord.Create(
            _validTenantId,
            _validFlockId,
            ValidRecordDate,
            ValidEggCount,
            string.Empty);

        // Assert
        dailyRecord.Should().NotBeNull();
        dailyRecord.Notes.Should().Be(string.Empty);
    }

    #endregion

    #region Update Tests

    [Fact]
    public void Update_WithValidData_ShouldUpdateEggCountAndNotes()
    {
        // Arrange
        var dailyRecord = DailyRecord.Create(
            _validTenantId,
            _validFlockId,
            ValidRecordDate,
            ValidEggCount,
            ValidNotes);
        var originalCreatedAt = dailyRecord.CreatedAt;
        var originalUpdatedAt = dailyRecord.UpdatedAt;
        Thread.Sleep(10);

        var newEggCount = 20;
        var newNotes = "Updated notes";

        // Act
        dailyRecord.Update(newEggCount, newNotes);

        // Assert
        dailyRecord.EggCount.Should().Be(newEggCount);
        dailyRecord.Notes.Should().Be(newNotes);
        dailyRecord.CreatedAt.Should().Be(originalCreatedAt);
        dailyRecord.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void Update_WithoutNotes_ShouldUpdateEggCountOnly()
    {
        // Arrange
        var dailyRecord = DailyRecord.Create(
            _validTenantId,
            _validFlockId,
            ValidRecordDate,
            ValidEggCount,
            ValidNotes);

        var newEggCount = 25;

        // Act
        dailyRecord.Update(newEggCount);

        // Assert
        dailyRecord.EggCount.Should().Be(newEggCount);
        dailyRecord.Notes.Should().BeNull();
    }

    [Fact]
    public void Update_WithNegativeEggCount_ShouldThrowArgumentException()
    {
        // Arrange
        var dailyRecord = DailyRecord.Create(
            _validTenantId,
            _validFlockId,
            ValidRecordDate,
            ValidEggCount);

        // Act
        var act = () => dailyRecord.Update(-1);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Egg count cannot be negative.*")
            .And.ParamName.Should().Be("eggCount");
    }

    [Fact]
    public void Update_WithNotesExceeding500Characters_ShouldThrowArgumentException()
    {
        // Arrange
        var dailyRecord = DailyRecord.Create(
            _validTenantId,
            _validFlockId,
            ValidRecordDate,
            ValidEggCount);

        var longNotes = new string('B', 501);

        // Act
        var act = () => dailyRecord.Update(10, longNotes);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Notes cannot exceed 500 characters.*")
            .And.ParamName.Should().Be("notes");
    }

    [Fact]
    public void Update_ShouldNotModifyRecordDate()
    {
        // Arrange
        var dailyRecord = DailyRecord.Create(
            _validTenantId,
            _validFlockId,
            ValidRecordDate,
            ValidEggCount);
        var originalRecordDate = dailyRecord.RecordDate;

        // Act
        dailyRecord.Update(20, "New notes");

        // Assert
        dailyRecord.RecordDate.Should().Be(originalRecordDate);
    }

    [Fact]
    public void Update_ShouldNotModifyTenantOrFlockIds()
    {
        // Arrange
        var dailyRecord = DailyRecord.Create(
            _validTenantId,
            _validFlockId,
            ValidRecordDate,
            ValidEggCount);

        // Act
        dailyRecord.Update(20, "New notes");

        // Assert
        dailyRecord.TenantId.Should().Be(_validTenantId);
        dailyRecord.FlockId.Should().Be(_validFlockId);
    }

    #endregion

    #region Date Handling Tests

    [Fact]
    public void Create_WithMultipleCallsOnSameDay_ShouldHaveSameRecordDate()
    {
        // Arrange
        var today = DateTime.UtcNow.Date;
        var timeVariant1 = today.AddHours(8).AddMinutes(30);
        var timeVariant2 = today.AddHours(17).AddMinutes(45);

        // Act
        var record1 = DailyRecord.Create(_validTenantId, _validFlockId, timeVariant1, 10);
        var record2 = DailyRecord.Create(_validTenantId, _validFlockId, timeVariant2, 12);

        // Assert
        record1.RecordDate.Should().Be(record2.RecordDate);
        record1.RecordDate.Should().Be(today);
    }

    #endregion
}
