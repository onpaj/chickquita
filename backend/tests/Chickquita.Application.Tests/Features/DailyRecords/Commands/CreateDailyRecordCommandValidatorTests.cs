using Chickquita.Application.Features.DailyRecords.Commands;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Chickquita.Application.Tests.Features.DailyRecords.Commands;

/// <summary>
/// Unit tests for CreateDailyRecordCommandValidator.
/// Tests cover all validation rules for daily record creation.
/// </summary>
public class CreateDailyRecordCommandValidatorTests
{
    private readonly CreateDailyRecordCommandValidator _validator;

    public CreateDailyRecordCommandValidatorTests()
    {
        _validator = new CreateDailyRecordCommandValidator();
    }

    #region FlockId Validation Tests

    [Fact]
    public void Validate_WithEmptyFlockId_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateDailyRecordCommand
        {
            FlockId = Guid.Empty,
            RecordDate = DateTime.UtcNow.Date,
            EggCount = 10
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FlockId)
            .WithErrorMessage("Flock ID is required.");
    }

    [Fact]
    public void Validate_WithValidFlockId_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new CreateDailyRecordCommand
        {
            FlockId = Guid.NewGuid(),
            RecordDate = DateTime.UtcNow.Date,
            EggCount = 10
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.FlockId);
    }

    #endregion

    #region RecordDate Validation Tests

    [Fact]
    public void Validate_WithDefaultRecordDate_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateDailyRecordCommand
        {
            FlockId = Guid.NewGuid(),
            RecordDate = default(DateTime),
            EggCount = 10
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RecordDate)
            .WithErrorMessage("Record date is required.");
    }

    [Fact]
    public void Validate_WithFutureRecordDate_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateDailyRecordCommand
        {
            FlockId = Guid.NewGuid(),
            RecordDate = DateTime.UtcNow.Date.AddDays(1),
            EggCount = 10
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RecordDate)
            .WithErrorMessage("Record date cannot be in the future.");
    }

    [Fact]
    public void Validate_WithTodayAsRecordDate_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new CreateDailyRecordCommand
        {
            FlockId = Guid.NewGuid(),
            RecordDate = DateTime.UtcNow.Date,
            EggCount = 10
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.RecordDate);
    }

    [Fact]
    public void Validate_WithPastRecordDate_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new CreateDailyRecordCommand
        {
            FlockId = Guid.NewGuid(),
            RecordDate = DateTime.UtcNow.Date.AddDays(-7),
            EggCount = 10
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.RecordDate);
    }

    #endregion

    #region EggCount Validation Tests

    [Fact]
    public void Validate_WithNegativeEggCount_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateDailyRecordCommand
        {
            FlockId = Guid.NewGuid(),
            RecordDate = DateTime.UtcNow.Date,
            EggCount = -5
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EggCount)
            .WithErrorMessage("Egg count cannot be negative.");
    }

    [Fact]
    public void Validate_WithZeroEggCount_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new CreateDailyRecordCommand
        {
            FlockId = Guid.NewGuid(),
            RecordDate = DateTime.UtcNow.Date,
            EggCount = 0
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.EggCount);
    }

    [Fact]
    public void Validate_WithPositiveEggCount_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new CreateDailyRecordCommand
        {
            FlockId = Guid.NewGuid(),
            RecordDate = DateTime.UtcNow.Date,
            EggCount = 50
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.EggCount);
    }

    [Fact]
    public void Validate_WithLargeEggCount_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new CreateDailyRecordCommand
        {
            FlockId = Guid.NewGuid(),
            RecordDate = DateTime.UtcNow.Date,
            EggCount = 1000
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.EggCount);
    }

    #endregion

    #region Notes Validation Tests

    [Fact]
    public void Validate_WithNullNotes_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new CreateDailyRecordCommand
        {
            FlockId = Guid.NewGuid(),
            RecordDate = DateTime.UtcNow.Date,
            EggCount = 10,
            Notes = null
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Notes);
    }

    [Fact]
    public void Validate_WithEmptyNotes_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new CreateDailyRecordCommand
        {
            FlockId = Guid.NewGuid(),
            RecordDate = DateTime.UtcNow.Date,
            EggCount = 10,
            Notes = string.Empty
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Notes);
    }

    [Fact]
    public void Validate_WithNotesExceeding500Characters_ShouldHaveValidationError()
    {
        // Arrange
        var longNotes = new string('A', 501);
        var command = new CreateDailyRecordCommand
        {
            FlockId = Guid.NewGuid(),
            RecordDate = DateTime.UtcNow.Date,
            EggCount = 10,
            Notes = longNotes
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Notes)
            .WithErrorMessage("Notes must not exceed 500 characters.");
    }

    [Fact]
    public void Validate_WithNotesExactly500Characters_ShouldNotHaveValidationError()
    {
        // Arrange
        var notes = new string('A', 500);
        var command = new CreateDailyRecordCommand
        {
            FlockId = Guid.NewGuid(),
            RecordDate = DateTime.UtcNow.Date,
            EggCount = 10,
            Notes = notes
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Notes);
    }

    [Fact]
    public void Validate_WithValidNotes_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new CreateDailyRecordCommand
        {
            FlockId = Guid.NewGuid(),
            RecordDate = DateTime.UtcNow.Date,
            EggCount = 10,
            Notes = "Sunny day, very productive hens"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Notes);
    }

    #endregion

    #region Complete Validation Tests

    [Fact]
    public void Validate_WithAllValidFields_ShouldNotHaveAnyValidationErrors()
    {
        // Arrange
        var command = new CreateDailyRecordCommand
        {
            FlockId = Guid.NewGuid(),
            RecordDate = DateTime.UtcNow.Date.AddDays(-1),
            EggCount = 25,
            Notes = "Good weather, all hens healthy"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithMultipleInvalidFields_ShouldHaveMultipleValidationErrors()
    {
        // Arrange
        var command = new CreateDailyRecordCommand
        {
            FlockId = Guid.Empty,
            RecordDate = DateTime.UtcNow.Date.AddDays(5),
            EggCount = -10,
            Notes = new string('A', 501)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FlockId);
        result.ShouldHaveValidationErrorFor(x => x.RecordDate);
        result.ShouldHaveValidationErrorFor(x => x.EggCount);
        result.ShouldHaveValidationErrorFor(x => x.Notes);
    }

    #endregion
}
