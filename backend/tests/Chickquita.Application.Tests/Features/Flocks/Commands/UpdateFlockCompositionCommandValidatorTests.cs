using Chickquita.Application.Features.Flocks.Commands;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Chickquita.Application.Tests.Features.Flocks.Commands;

/// <summary>
/// Unit tests for UpdateFlockCompositionCommandValidator.
/// Tests cover FlockId and non-negative count validation rules.
/// </summary>
public class UpdateFlockCompositionCommandValidatorTests
{
    private readonly UpdateFlockCompositionCommandValidator _validator;

    public UpdateFlockCompositionCommandValidatorTests()
    {
        _validator = new UpdateFlockCompositionCommandValidator();
    }

    #region FlockId Validation Tests

    [Fact]
    public void Validate_WithEmptyFlockId_ShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateFlockCompositionCommand
        {
            FlockId = Guid.Empty,
            Hens = 10,
            Roosters = 2,
            Chicks = 5
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
        var command = new UpdateFlockCompositionCommand
        {
            FlockId = Guid.NewGuid(),
            Hens = 10,
            Roosters = 2,
            Chicks = 5
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.FlockId);
    }

    #endregion

    #region Hens Validation Tests

    [Fact]
    public void Validate_WithNegativeHens_ShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateFlockCompositionCommand
        {
            FlockId = Guid.NewGuid(),
            Hens = -1,
            Roosters = 2,
            Chicks = 5
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Hens)
            .WithErrorMessage("Hens count cannot be negative.");
    }

    [Fact]
    public void Validate_WithZeroHens_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new UpdateFlockCompositionCommand
        {
            FlockId = Guid.NewGuid(),
            Hens = 0,
            Roosters = 2,
            Chicks = 5
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Hens);
    }

    [Fact]
    public void Validate_WithPositiveHens_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new UpdateFlockCompositionCommand
        {
            FlockId = Guid.NewGuid(),
            Hens = 15,
            Roosters = 2,
            Chicks = 5
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Hens);
    }

    #endregion

    #region Roosters Validation Tests

    [Fact]
    public void Validate_WithNegativeRoosters_ShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateFlockCompositionCommand
        {
            FlockId = Guid.NewGuid(),
            Hens = 10,
            Roosters = -1,
            Chicks = 5
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Roosters)
            .WithErrorMessage("Roosters count cannot be negative.");
    }

    [Fact]
    public void Validate_WithZeroRoosters_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new UpdateFlockCompositionCommand
        {
            FlockId = Guid.NewGuid(),
            Hens = 10,
            Roosters = 0,
            Chicks = 5
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Roosters);
    }

    #endregion

    #region Chicks Validation Tests

    [Fact]
    public void Validate_WithNegativeChicks_ShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateFlockCompositionCommand
        {
            FlockId = Guid.NewGuid(),
            Hens = 10,
            Roosters = 2,
            Chicks = -1
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Chicks)
            .WithErrorMessage("Chicks count cannot be negative.");
    }

    [Fact]
    public void Validate_WithZeroChicks_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new UpdateFlockCompositionCommand
        {
            FlockId = Guid.NewGuid(),
            Hens = 10,
            Roosters = 2,
            Chicks = 0
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Chicks);
    }

    #endregion

    #region Complete Validation Tests

    [Fact]
    public void Validate_WithAllValidFields_ShouldNotHaveAnyValidationErrors()
    {
        // Arrange
        var command = new UpdateFlockCompositionCommand
        {
            FlockId = Guid.NewGuid(),
            Hens = 20,
            Roosters = 4,
            Chicks = 8
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
        var command = new UpdateFlockCompositionCommand
        {
            FlockId = Guid.Empty,
            Hens = -5,
            Roosters = -1,
            Chicks = -3
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FlockId);
        result.ShouldHaveValidationErrorFor(x => x.Hens);
        result.ShouldHaveValidationErrorFor(x => x.Roosters);
        result.ShouldHaveValidationErrorFor(x => x.Chicks);
    }

    #endregion
}
