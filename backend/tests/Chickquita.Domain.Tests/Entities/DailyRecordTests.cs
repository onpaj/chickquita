using Chickquita.Domain.Common;
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
        var result = DailyRecord.Create(
            _validTenantId,
            _validFlockId,
            ValidRecordDate,
            ValidEggCount,
            ValidNotes);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var dailyRecord = result.Value;
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
        var result = DailyRecord.Create(
            _validTenantId,
            _validFlockId,
            ValidRecordDate,
            ValidEggCount);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var dailyRecord = result.Value;
        dailyRecord.Should().NotBeNull();
        dailyRecord.Notes.Should().BeNull();
    }

    [Fact]
    public void Create_WithZeroEggCount_ShouldSucceed()
    {
        // Arrange & Act
        var result = DailyRecord.Create(
            _validTenantId,
            _validFlockId,
            ValidRecordDate,
            0);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var dailyRecord = result.Value;
        dailyRecord.Should().NotBeNull();
        dailyRecord.EggCount.Should().Be(0);
    }

    [Fact]
    public void Create_ShouldGenerateUniqueIds()
    {
        // Arrange & Act
        var record1 = DailyRecord.Create(_validTenantId, _validFlockId, ValidRecordDate, 10).Value;
        var record2 = DailyRecord.Create(_validTenantId, _validFlockId, ValidRecordDate.AddDays(-1), 12).Value;

        // Assert
        record1.Id.Should().NotBe(record2.Id);
    }

    [Fact]
    public void Create_ShouldNormalizeDateToMidnight()
    {
        // Arrange
        var dateWithTime = DateTime.UtcNow.AddDays(-1).AddHours(14).AddMinutes(30);

        // Act
        var result = DailyRecord.Create(
            _validTenantId,
            _validFlockId,
            dateWithTime,
            ValidEggCount);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var dailyRecord = result.Value;
        dailyRecord.RecordDate.Hour.Should().Be(0);
        dailyRecord.RecordDate.Minute.Should().Be(0);
        dailyRecord.RecordDate.Second.Should().Be(0);
        dailyRecord.RecordDate.Millisecond.Should().Be(0);
    }

    #endregion

    #region Validation Tests - Tenant and Flock ID

    [Fact]
    public void Create_WithEmptyTenantId_ShouldReturnFailure()
    {
        // Arrange
        var emptyTenantId = Guid.Empty;

        // Act
        var result = DailyRecord.Create(
            emptyTenantId,
            _validFlockId,
            ValidRecordDate,
            ValidEggCount);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Tenant ID cannot be empty");
    }

    [Fact]
    public void Create_WithEmptyFlockId_ShouldReturnFailure()
    {
        // Arrange
        var emptyFlockId = Guid.Empty;

        // Act
        var result = DailyRecord.Create(
            _validTenantId,
            emptyFlockId,
            ValidRecordDate,
            ValidEggCount);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Flock ID cannot be empty");
    }

    #endregion

    #region Validation Tests - Record Date

    [Fact]
    public void Create_WithFutureRecordDate_ShouldReturnFailure()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.Date.AddDays(1);

        // Act
        var result = DailyRecord.Create(
            _validTenantId,
            _validFlockId,
            futureDate,
            ValidEggCount);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Record date cannot be in the future");
    }

    [Fact]
    public void Create_WithTodayRecordDate_ShouldSucceed()
    {
        // Arrange
        var today = DateTime.UtcNow.Date;

        // Act
        var result = DailyRecord.Create(
            _validTenantId,
            _validFlockId,
            today,
            ValidEggCount);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var dailyRecord = result.Value;
        dailyRecord.Should().NotBeNull();
        dailyRecord.RecordDate.Should().Be(today);
    }

    [Fact]
    public void Create_WithPastRecordDate_ShouldSucceed()
    {
        // Arrange
        var pastDate = DateTime.UtcNow.Date.AddYears(-1);

        // Act
        var result = DailyRecord.Create(
            _validTenantId,
            _validFlockId,
            pastDate,
            ValidEggCount);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var dailyRecord = result.Value;
        dailyRecord.Should().NotBeNull();
        dailyRecord.RecordDate.Should().Be(pastDate);
    }

    [Fact]
    public void Create_WithLocalTime_ShouldConvertToUtc()
    {
        // Arrange
        var localDate = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Local);

        // Act
        var result = DailyRecord.Create(
            _validTenantId,
            _validFlockId,
            localDate,
            ValidEggCount);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.RecordDate.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void Create_WithUnspecifiedKind_ShouldConvertToUtc()
    {
        // Arrange
        var unspecifiedDate = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Unspecified);

        // Act
        var result = DailyRecord.Create(
            _validTenantId,
            _validFlockId,
            unspecifiedDate,
            ValidEggCount);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.RecordDate.Kind.Should().Be(DateTimeKind.Utc);
    }

    #endregion

    #region Validation Tests - Egg Count

    [Fact]
    public void Create_WithNegativeEggCount_ShouldReturnFailure()
    {
        // Arrange & Act
        var result = DailyRecord.Create(
            _validTenantId,
            _validFlockId,
            ValidRecordDate,
            -1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Egg count cannot be negative");
    }

    [Fact]
    public void Create_WithLargeEggCount_ShouldSucceed()
    {
        // Arrange & Act
        var result = DailyRecord.Create(
            _validTenantId,
            _validFlockId,
            ValidRecordDate,
            1000);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var dailyRecord = result.Value;
        dailyRecord.Should().NotBeNull();
        dailyRecord.EggCount.Should().Be(1000);
    }

    #endregion

    #region Validation Tests - Notes

    [Fact]
    public void Create_WithNotesExceeding500Characters_ShouldReturnFailure()
    {
        // Arrange
        var longNotes = new string('A', 501);

        // Act
        var result = DailyRecord.Create(
            _validTenantId,
            _validFlockId,
            ValidRecordDate,
            ValidEggCount,
            longNotes);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Notes cannot exceed 500 characters");
    }

    [Fact]
    public void Create_WithNotesExactly500Characters_ShouldSucceed()
    {
        // Arrange
        var notes = new string('A', 500);

        // Act
        var result = DailyRecord.Create(
            _validTenantId,
            _validFlockId,
            ValidRecordDate,
            ValidEggCount,
            notes);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var dailyRecord = result.Value;
        dailyRecord.Should().NotBeNull();
        dailyRecord.Notes.Should().Be(notes);
        dailyRecord.Notes!.Length.Should().Be(500);
    }

    [Fact]
    public void Create_WithEmptyStringNotes_ShouldSucceed()
    {
        // Arrange & Act
        var result = DailyRecord.Create(
            _validTenantId,
            _validFlockId,
            ValidRecordDate,
            ValidEggCount,
            string.Empty);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var dailyRecord = result.Value;
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
            ValidNotes).Value;
        var originalCreatedAt = dailyRecord.CreatedAt;
        var originalUpdatedAt = dailyRecord.UpdatedAt;
        Thread.Sleep(10);

        var newEggCount = 20;
        var newNotes = "Updated notes";

        // Act
        var updateResult = dailyRecord.Update(newEggCount, newNotes);

        // Assert
        updateResult.IsSuccess.Should().BeTrue();
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
            ValidNotes).Value;

        var newEggCount = 25;

        // Act
        var updateResult = dailyRecord.Update(newEggCount);

        // Assert
        updateResult.IsSuccess.Should().BeTrue();
        dailyRecord.EggCount.Should().Be(newEggCount);
        dailyRecord.Notes.Should().BeNull();
    }

    [Fact]
    public void Update_WithNegativeEggCount_ShouldReturnFailure()
    {
        // Arrange
        var dailyRecord = DailyRecord.Create(
            _validTenantId,
            _validFlockId,
            ValidRecordDate,
            ValidEggCount).Value;

        // Act
        var updateResult = dailyRecord.Update(-1);

        // Assert
        updateResult.IsFailure.Should().BeTrue();
        updateResult.Error.Message.Should().Contain("Egg count cannot be negative");
    }

    [Fact]
    public void Update_WithNotesExceeding500Characters_ShouldReturnFailure()
    {
        // Arrange
        var dailyRecord = DailyRecord.Create(
            _validTenantId,
            _validFlockId,
            ValidRecordDate,
            ValidEggCount).Value;

        var longNotes = new string('B', 501);

        // Act
        var updateResult = dailyRecord.Update(10, longNotes);

        // Assert
        updateResult.IsFailure.Should().BeTrue();
        updateResult.Error.Message.Should().Contain("Notes cannot exceed 500 characters");
    }

    [Fact]
    public void Update_ShouldNotModifyRecordDate()
    {
        // Arrange
        var dailyRecord = DailyRecord.Create(
            _validTenantId,
            _validFlockId,
            ValidRecordDate,
            ValidEggCount).Value;
        var originalRecordDate = dailyRecord.RecordDate;

        // Act
        var updateResult = dailyRecord.Update(20, "New notes");

        // Assert
        updateResult.IsSuccess.Should().BeTrue();
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
            ValidEggCount).Value;

        // Act
        var updateResult = dailyRecord.Update(20, "New notes");

        // Assert
        updateResult.IsSuccess.Should().BeTrue();
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
        var record1 = DailyRecord.Create(_validTenantId, _validFlockId, timeVariant1, 10).Value;
        var record2 = DailyRecord.Create(_validTenantId, _validFlockId, timeVariant2, 12).Value;

        // Assert
        record1.RecordDate.Should().Be(record2.RecordDate);
        record1.RecordDate.Should().Be(today);
    }

    #endregion
}
