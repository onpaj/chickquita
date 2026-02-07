using Chickquita.Application.Features.DailyRecords.Commands;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Chickquita.Application.Tests.Features.DailyRecords.Commands;

/// <summary>
/// Unit tests for UpdateDailyRecordCommandValidator.
/// Tests validation rules for all command properties.
/// </summary>
public class UpdateDailyRecordCommandValidatorTests
{
    private readonly UpdateDailyRecordCommandValidator _validator;

    public UpdateDailyRecordCommandValidatorTests()
    {
        _validator = new UpdateDailyRecordCommandValidator();
    }

    #region Id Validation Tests

    [Fact]
    public void Validate_WithValidId_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new UpdateDailyRecordCommand
        {
            Id = Guid.NewGuid(),
            EggCount = 10
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void Validate_WithEmptyId_ShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateDailyRecordCommand
        {
            Id = Guid.Empty,
            EggCount = 10
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Daily record ID is required.");
    }

    #endregion

    #region EggCount Validation Tests

    [Fact]
    public void Validate_WithZeroEggCount_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new UpdateDailyRecordCommand
        {
            Id = Guid.NewGuid(),
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
        var command = new UpdateDailyRecordCommand
        {
            Id = Guid.NewGuid(),
            EggCount = 50
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.EggCount);
    }

    [Fact]
    public void Validate_WithNegativeEggCount_ShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateDailyRecordCommand
        {
            Id = Guid.NewGuid(),
            EggCount = -1
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EggCount)
            .WithErrorMessage("Egg count cannot be negative.");
    }

    #endregion

    #region Notes Validation Tests

    [Fact]
    public void Validate_WithNullNotes_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new UpdateDailyRecordCommand
        {
            Id = Guid.NewGuid(),
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
        var command = new UpdateDailyRecordCommand
        {
            Id = Guid.NewGuid(),
            EggCount = 10,
            Notes = string.Empty
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Notes);
    }

    [Fact]
    public void Validate_WithNotesAt500Characters_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new UpdateDailyRecordCommand
        {
            Id = Guid.NewGuid(),
            EggCount = 10,
            Notes = new string('A', 500)
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
        var command = new UpdateDailyRecordCommand
        {
            Id = Guid.NewGuid(),
            EggCount = 10,
            Notes = new string('A', 501)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Notes)
            .WithErrorMessage("Notes must not exceed 500 characters.");
    }

    [Fact]
    public void Validate_WithValidNotes_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new UpdateDailyRecordCommand
        {
            Id = Guid.NewGuid(),
            EggCount = 10,
            Notes = "Good weather today, productive hens"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Notes);
    }

    #endregion

    #region Complete Command Validation Tests

    [Fact]
    public void Validate_WithAllValidFields_ShouldNotHaveAnyValidationErrors()
    {
        // Arrange
        var command = new UpdateDailyRecordCommand
        {
            Id = Guid.NewGuid(),
            EggCount = 25,
            Notes = "Excellent production today"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithMultipleErrors_ShouldHaveAllValidationErrors()
    {
        // Arrange
        var command = new UpdateDailyRecordCommand
        {
            Id = Guid.Empty,
            EggCount = -10,
            Notes = new string('X', 600)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id);
        result.ShouldHaveValidationErrorFor(x => x.EggCount);
        result.ShouldHaveValidationErrorFor(x => x.Notes);
    }

    #endregion
}
