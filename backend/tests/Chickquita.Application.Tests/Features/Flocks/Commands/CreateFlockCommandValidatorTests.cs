using Chickquita.Application.Features.Flocks.Commands;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Chickquita.Application.Tests.Features.Flocks.Commands;

/// <summary>
/// Unit tests for CreateFlockCommandValidator.
/// Tests cover all validation rules including at-least-one-count business rule.
/// </summary>
public class CreateFlockCommandValidatorTests
{
    private readonly CreateFlockCommandValidator _validator;

    public CreateFlockCommandValidatorTests()
    {
        _validator = new CreateFlockCommandValidator();
    }

    #region CoopId Validation Tests

    [Fact]
    public void Validate_WithEmptyCoopId_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateFlockCommand
        {
            CoopId = Guid.Empty,
            Identifier = "Test Flock",
            HatchDate = DateTime.UtcNow.AddDays(-30),
            InitialHens = 10,
            InitialRoosters = 2,
            InitialChicks = 0
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CoopId)
            .WithErrorMessage("Coop ID is required.");
    }

    [Fact]
    public void Validate_WithValidCoopId_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new CreateFlockCommand
        {
            CoopId = Guid.NewGuid(),
            Identifier = "Test Flock",
            HatchDate = DateTime.UtcNow.AddDays(-30),
            InitialHens = 10,
            InitialRoosters = 2,
            InitialChicks = 0
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.CoopId);
    }

    #endregion

    #region Identifier Validation Tests

    [Fact]
    public void Validate_WithEmptyIdentifier_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateFlockCommand
        {
            CoopId = Guid.NewGuid(),
            Identifier = string.Empty,
            HatchDate = DateTime.UtcNow.AddDays(-30),
            InitialHens = 10,
            InitialRoosters = 2,
            InitialChicks = 0
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
        var command = new CreateFlockCommand
        {
            CoopId = Guid.NewGuid(),
            Identifier = "   ",
            HatchDate = DateTime.UtcNow.AddDays(-30),
            InitialHens = 10,
            InitialRoosters = 2,
            InitialChicks = 0
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
        var command = new CreateFlockCommand
        {
            CoopId = Guid.NewGuid(),
            Identifier = longIdentifier,
            HatchDate = DateTime.UtcNow.AddDays(-30),
            InitialHens = 10,
            InitialRoosters = 2,
            InitialChicks = 0
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
        var command = new CreateFlockCommand
        {
            CoopId = Guid.NewGuid(),
            Identifier = identifier,
            HatchDate = DateTime.UtcNow.AddDays(-30),
            InitialHens = 10,
            InitialRoosters = 2,
            InitialChicks = 0
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
        var command = new CreateFlockCommand
        {
            CoopId = Guid.NewGuid(),
            Identifier = "Spring 2024 Batch",
            HatchDate = DateTime.UtcNow.AddDays(-30),
            InitialHens = 10,
            InitialRoosters = 2,
            InitialChicks = 0
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
        var command = new CreateFlockCommand
        {
            CoopId = Guid.NewGuid(),
            Identifier = "Test Flock",
            HatchDate = default(DateTime),
            InitialHens = 10,
            InitialRoosters = 2,
            InitialChicks = 0
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
        var command = new CreateFlockCommand
        {
            CoopId = Guid.NewGuid(),
            Identifier = "Test Flock",
            HatchDate = DateTime.UtcNow.AddDays(10),
            InitialHens = 10,
            InitialRoosters = 2,
            InitialChicks = 0
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
        var command = new CreateFlockCommand
        {
            CoopId = Guid.NewGuid(),
            Identifier = "Test Flock",
            HatchDate = DateTime.UtcNow.AddSeconds(-1),
            InitialHens = 10,
            InitialRoosters = 2,
            InitialChicks = 0
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
        var command = new CreateFlockCommand
        {
            CoopId = Guid.NewGuid(),
            Identifier = "Test Flock",
            HatchDate = DateTime.UtcNow.AddDays(-60),
            InitialHens = 10,
            InitialRoosters = 2,
            InitialChicks = 0
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.HatchDate);
    }

    #endregion

    #region Animal Count Validation Tests - Non-Negative

    [Fact]
    public void Validate_WithNegativeInitialHens_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateFlockCommand
        {
            CoopId = Guid.NewGuid(),
            Identifier = "Test Flock",
            HatchDate = DateTime.UtcNow.AddDays(-30),
            InitialHens = -5,
            InitialRoosters = 2,
            InitialChicks = 0
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.InitialHens)
            .WithErrorMessage("Initial hens count cannot be negative.");
    }

    [Fact]
    public void Validate_WithNegativeInitialRoosters_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateFlockCommand
        {
            CoopId = Guid.NewGuid(),
            Identifier = "Test Flock",
            HatchDate = DateTime.UtcNow.AddDays(-30),
            InitialHens = 10,
            InitialRoosters = -2,
            InitialChicks = 0
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.InitialRoosters)
            .WithErrorMessage("Initial roosters count cannot be negative.");
    }

    [Fact]
    public void Validate_WithNegativeInitialChicks_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateFlockCommand
        {
            CoopId = Guid.NewGuid(),
            Identifier = "Test Flock",
            HatchDate = DateTime.UtcNow.AddDays(-30),
            InitialHens = 10,
            InitialRoosters = 2,
            InitialChicks = -3
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.InitialChicks)
            .WithErrorMessage("Initial chicks count cannot be negative.");
    }

    [Fact]
    public void Validate_WithZeroInitialHens_ShouldNotHaveValidationErrorForHens()
    {
        // Arrange
        var command = new CreateFlockCommand
        {
            CoopId = Guid.NewGuid(),
            Identifier = "Test Flock",
            HatchDate = DateTime.UtcNow.AddDays(-30),
            InitialHens = 0,
            InitialRoosters = 5,
            InitialChicks = 0
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.InitialHens);
    }

    [Fact]
    public void Validate_WithZeroInitialRoosters_ShouldNotHaveValidationErrorForRoosters()
    {
        // Arrange
        var command = new CreateFlockCommand
        {
            CoopId = Guid.NewGuid(),
            Identifier = "Test Flock",
            HatchDate = DateTime.UtcNow.AddDays(-30),
            InitialHens = 10,
            InitialRoosters = 0,
            InitialChicks = 0
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.InitialRoosters);
    }

    [Fact]
    public void Validate_WithZeroInitialChicks_ShouldNotHaveValidationErrorForChicks()
    {
        // Arrange
        var command = new CreateFlockCommand
        {
            CoopId = Guid.NewGuid(),
            Identifier = "Test Flock",
            HatchDate = DateTime.UtcNow.AddDays(-30),
            InitialHens = 10,
            InitialRoosters = 2,
            InitialChicks = 0
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.InitialChicks);
    }

    #endregion

    #region At-Least-One-Count Business Rule Tests

    [Fact]
    public void Validate_WithAllCountsZero_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateFlockCommand
        {
            CoopId = Guid.NewGuid(),
            Identifier = "Empty Flock",
            HatchDate = DateTime.UtcNow.AddDays(-30),
            InitialHens = 0,
            InitialRoosters = 0,
            InitialChicks = 0
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorMessage("At least one animal type must have a count greater than 0.");
    }

    [Fact]
    public void Validate_WithOnlyHensGreaterThanZero_ShouldNotHaveValidationErrorForAtLeastOneCount()
    {
        // Arrange
        var command = new CreateFlockCommand
        {
            CoopId = Guid.NewGuid(),
            Identifier = "Hens Only",
            HatchDate = DateTime.UtcNow.AddDays(-30),
            InitialHens = 1,
            InitialRoosters = 0,
            InitialChicks = 0
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x);
    }

    [Fact]
    public void Validate_WithOnlyRoostersGreaterThanZero_ShouldNotHaveValidationErrorForAtLeastOneCount()
    {
        // Arrange
        var command = new CreateFlockCommand
        {
            CoopId = Guid.NewGuid(),
            Identifier = "Roosters Only",
            HatchDate = DateTime.UtcNow.AddDays(-30),
            InitialHens = 0,
            InitialRoosters = 1,
            InitialChicks = 0
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x);
    }

    [Fact]
    public void Validate_WithOnlyChicksGreaterThanZero_ShouldNotHaveValidationErrorForAtLeastOneCount()
    {
        // Arrange
        var command = new CreateFlockCommand
        {
            CoopId = Guid.NewGuid(),
            Identifier = "Chicks Only",
            HatchDate = DateTime.UtcNow.AddDays(-30),
            InitialHens = 0,
            InitialRoosters = 0,
            InitialChicks = 1
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x);
    }

    [Fact]
    public void Validate_WithMultipleCountsGreaterThanZero_ShouldNotHaveValidationErrorForAtLeastOneCount()
    {
        // Arrange
        var command = new CreateFlockCommand
        {
            CoopId = Guid.NewGuid(),
            Identifier = "Mixed Flock",
            HatchDate = DateTime.UtcNow.AddDays(-30),
            InitialHens = 10,
            InitialRoosters = 2,
            InitialChicks = 5
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x);
    }

    [Fact]
    public void Validate_WithLargeHensCount_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new CreateFlockCommand
        {
            CoopId = Guid.NewGuid(),
            Identifier = "Large Flock",
            HatchDate = DateTime.UtcNow.AddDays(-30),
            InitialHens = 1000,
            InitialRoosters = 0,
            InitialChicks = 0
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion

    #region Notes Validation Tests

    [Fact]
    public void Validate_WithNullNotes_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new CreateFlockCommand
        {
            CoopId = Guid.NewGuid(),
            Identifier = "Test Flock",
            HatchDate = DateTime.UtcNow.AddDays(-30),
            InitialHens = 10,
            InitialRoosters = 2,
            InitialChicks = 0,
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
        var command = new CreateFlockCommand
        {
            CoopId = Guid.NewGuid(),
            Identifier = "Test Flock",
            HatchDate = DateTime.UtcNow.AddDays(-30),
            InitialHens = 10,
            InitialRoosters = 2,
            InitialChicks = 0,
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
        var command = new CreateFlockCommand
        {
            CoopId = Guid.NewGuid(),
            Identifier = "Test Flock",
            HatchDate = DateTime.UtcNow.AddDays(-30),
            InitialHens = 10,
            InitialRoosters = 2,
            InitialChicks = 0,
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
        var command = new CreateFlockCommand
        {
            CoopId = Guid.NewGuid(),
            Identifier = "Test Flock",
            HatchDate = DateTime.UtcNow.AddDays(-30),
            InitialHens = 10,
            InitialRoosters = 2,
            InitialChicks = 0,
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
        var command = new CreateFlockCommand
        {
            CoopId = Guid.NewGuid(),
            Identifier = "Test Flock",
            HatchDate = DateTime.UtcNow.AddDays(-30),
            InitialHens = 10,
            InitialRoosters = 2,
            InitialChicks = 0,
            Notes = "First batch - very healthy birds"
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
        var command = new CreateFlockCommand
        {
            CoopId = Guid.NewGuid(),
            Identifier = "Spring 2024 Batch",
            HatchDate = DateTime.UtcNow.AddDays(-45),
            InitialHens = 15,
            InitialRoosters = 3,
            InitialChicks = 10,
            Notes = "Healthy batch from reliable supplier"
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
        var command = new CreateFlockCommand
        {
            CoopId = Guid.Empty,
            Identifier = string.Empty,
            HatchDate = DateTime.UtcNow.AddDays(10),
            InitialHens = -5,
            InitialRoosters = -2,
            InitialChicks = -3,
            Notes = new string('A', 501)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CoopId);
        result.ShouldHaveValidationErrorFor(x => x.Identifier);
        result.ShouldHaveValidationErrorFor(x => x.HatchDate);
        result.ShouldHaveValidationErrorFor(x => x.InitialHens);
        result.ShouldHaveValidationErrorFor(x => x.InitialRoosters);
        result.ShouldHaveValidationErrorFor(x => x.InitialChicks);
        result.ShouldHaveValidationErrorFor(x => x.Notes);
    }

    #endregion
}
