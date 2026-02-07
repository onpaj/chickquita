using Chickquita.Application.Features.Flocks.Commands;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Chickquita.Application.Tests.Features.Flocks.Commands;

/// <summary>
/// Unit tests for UpdateFlockCommandValidator.
/// Tests cover all validation rules for identifier and hatch date.
/// </summary>
public class UpdateFlockCommandValidatorTests
{
    private readonly UpdateFlockCommandValidator _validator;

    public UpdateFlockCommandValidatorTests()
    {
        _validator = new UpdateFlockCommandValidator();
    }

    #region FlockId Validation Tests

    [Fact]
    public void Validate_WithEmptyFlockId_ShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateFlockCommand
        {
            FlockId = Guid.Empty,
            Identifier = "Test Flock",
            HatchDate = DateTime.UtcNow.AddDays(-30)
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
        var command = new UpdateFlockCommand
        {
            FlockId = Guid.NewGuid(),
            Identifier = "Test Flock",
            HatchDate = DateTime.UtcNow.AddDays(-30)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.FlockId);
    }

    #endregion

    #region Identifier Validation Tests

    [Fact]
    public void Validate_WithEmptyIdentifier_ShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateFlockCommand
        {
            FlockId = Guid.NewGuid(),
            Identifier = string.Empty,
            HatchDate = DateTime.UtcNow.AddDays(-30)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Identifier)
            .WithErrorMessage("Flock identifier is required.");
    }

    [Fact]
    public void Validate_WithWhitespaceIdentifier_ShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateFlockCommand
        {
            FlockId = Guid.NewGuid(),
            Identifier = "   ",
            HatchDate = DateTime.UtcNow.AddDays(-30)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Identifier)
            .WithErrorMessage("Flock identifier is required.");
    }

    [Fact]
    public void Validate_WithIdentifierExceeding50Characters_ShouldHaveValidationError()
    {
        // Arrange
        var longIdentifier = new string('A', 51);
        var command = new UpdateFlockCommand
        {
            FlockId = Guid.NewGuid(),
            Identifier = longIdentifier,
            HatchDate = DateTime.UtcNow.AddDays(-30)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Identifier)
            .WithErrorMessage("Flock identifier must not exceed 50 characters.");
    }

    [Fact]
    public void Validate_WithIdentifierExactly50Characters_ShouldNotHaveValidationError()
    {
        // Arrange
        var identifier = new string('A', 50);
        var command = new UpdateFlockCommand
        {
            FlockId = Guid.NewGuid(),
            Identifier = identifier,
            HatchDate = DateTime.UtcNow.AddDays(-30)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Identifier);
    }

    [Fact]
    public void Validate_WithValidIdentifier_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new UpdateFlockCommand
        {
            FlockId = Guid.NewGuid(),
            Identifier = "Spring 2024 Batch",
            HatchDate = DateTime.UtcNow.AddDays(-30)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Identifier);
    }

    #endregion

    #region HatchDate Validation Tests

    [Fact]
    public void Validate_WithDefaultHatchDate_ShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateFlockCommand
        {
            FlockId = Guid.NewGuid(),
            Identifier = "Test Flock",
            HatchDate = default(DateTime)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.HatchDate)
            .WithErrorMessage("Hatch date is required.");
    }

    [Fact]
    public void Validate_WithFutureHatchDate_ShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateFlockCommand
        {
            FlockId = Guid.NewGuid(),
            Identifier = "Test Flock",
            HatchDate = DateTime.UtcNow.AddDays(10)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.HatchDate)
            .WithErrorMessage("Hatch date cannot be in the future.");
    }

    [Fact]
    public void Validate_WithTodayAsHatchDate_ShouldNotHaveValidationError()
    {
        // Arrange
        // Use a date 1 second in the past to avoid race conditions with DateTime.UtcNow comparison
        var command = new UpdateFlockCommand
        {
            FlockId = Guid.NewGuid(),
            Identifier = "Test Flock",
            HatchDate = DateTime.UtcNow.AddSeconds(-1)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.HatchDate);
    }

    [Fact]
    public void Validate_WithPastHatchDate_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new UpdateFlockCommand
        {
            FlockId = Guid.NewGuid(),
            Identifier = "Test Flock",
            HatchDate = DateTime.UtcNow.AddDays(-60)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.HatchDate);
    }

    #endregion

    #region Complete Validation Tests

    [Fact]
    public void Validate_WithAllValidFields_ShouldNotHaveAnyValidationErrors()
    {
        // Arrange
        var command = new UpdateFlockCommand
        {
            FlockId = Guid.NewGuid(),
            Identifier = "Spring 2024 Batch",
            HatchDate = DateTime.UtcNow.AddDays(-45)
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
        var command = new UpdateFlockCommand
        {
            FlockId = Guid.Empty,
            Identifier = string.Empty,
            HatchDate = DateTime.UtcNow.AddDays(10)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FlockId);
        result.ShouldHaveValidationErrorFor(x => x.Identifier);
        result.ShouldHaveValidationErrorFor(x => x.HatchDate);
    }

    #endregion
}
