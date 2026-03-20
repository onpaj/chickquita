using Chickquita.Application.Features.EggSales.Commands.Update;
using FluentValidation.TestHelper;

namespace Chickquita.Application.Tests.Features.EggSales.Commands;

/// <summary>
/// Unit tests for UpdateEggSaleCommandValidator.
/// Tests all validation rules for the UpdateEggSaleCommand.
/// </summary>
public class UpdateEggSaleCommandValidatorTests
{
    private readonly UpdateEggSaleCommandValidator _validator;

    public UpdateEggSaleCommandValidatorTests()
    {
        _validator = new UpdateEggSaleCommandValidator();
    }

    #region Id Validation Tests

    [Fact]
    public void Validate_WithValidId_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new UpdateEggSaleCommand
        {
            Id = Guid.NewGuid(),
            Date = DateTime.UtcNow.Date,
            Quantity = 100,
            PricePerUnit = 5.00m
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
        var command = new UpdateEggSaleCommand
        {
            Id = Guid.Empty,
            Date = DateTime.UtcNow.Date,
            Quantity = 100,
            PricePerUnit = 5.00m
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Egg sale ID is required.");
    }

    #endregion

    #region Date Validation Tests

    [Fact]
    public void Validate_WithValidDate_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new UpdateEggSaleCommand
        {
            Id = Guid.NewGuid(),
            Date = DateTime.UtcNow.Date,
            Quantity = 100,
            PricePerUnit = 5.00m
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Date);
    }

    [Fact]
    public void Validate_WithDefaultDate_ShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateEggSaleCommand
        {
            Id = Guid.NewGuid(),
            Date = default,
            Quantity = 100,
            PricePerUnit = 5.00m
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Date)
            .WithErrorMessage("Sale date is required.");
    }

    #endregion

    #region Quantity Validation Tests

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(10000)]
    public void Validate_WithValidQuantity_ShouldNotHaveValidationError(int quantity)
    {
        // Arrange
        var command = new UpdateEggSaleCommand
        {
            Id = Guid.NewGuid(),
            Date = DateTime.UtcNow.Date,
            Quantity = quantity,
            PricePerUnit = 5.00m
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Quantity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_WithZeroOrNegativeQuantity_ShouldHaveValidationError(int quantity)
    {
        // Arrange
        var command = new UpdateEggSaleCommand
        {
            Id = Guid.NewGuid(),
            Date = DateTime.UtcNow.Date,
            Quantity = quantity,
            PricePerUnit = 5.00m
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Quantity)
            .WithErrorMessage("Quantity must be greater than zero.");
    }

    #endregion

    #region PricePerUnit Validation Tests

    [Theory]
    [InlineData(0)]
    [InlineData(5.00)]
    [InlineData(100.99)]
    public void Validate_WithValidPricePerUnit_ShouldNotHaveValidationError(double price)
    {
        // Arrange
        var command = new UpdateEggSaleCommand
        {
            Id = Guid.NewGuid(),
            Date = DateTime.UtcNow.Date,
            Quantity = 100,
            PricePerUnit = (decimal)price
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PricePerUnit);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(-1.00)]
    [InlineData(-100.00)]
    public void Validate_WithNegativePricePerUnit_ShouldHaveValidationError(double price)
    {
        // Arrange
        var command = new UpdateEggSaleCommand
        {
            Id = Guid.NewGuid(),
            Date = DateTime.UtcNow.Date,
            Quantity = 100,
            PricePerUnit = (decimal)price
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PricePerUnit)
            .WithErrorMessage("Price per unit cannot be negative.");
    }

    #endregion

    #region BuyerName Validation Tests

    [Fact]
    public void Validate_WithNullBuyerName_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new UpdateEggSaleCommand
        {
            Id = Guid.NewGuid(),
            Date = DateTime.UtcNow.Date,
            Quantity = 100,
            PricePerUnit = 5.00m,
            BuyerName = null
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.BuyerName);
    }

    [Fact]
    public void Validate_WithValidBuyerName_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new UpdateEggSaleCommand
        {
            Id = Guid.NewGuid(),
            Date = DateTime.UtcNow.Date,
            Quantity = 100,
            PricePerUnit = 5.00m,
            BuyerName = "Updated Market"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.BuyerName);
    }

    [Fact]
    public void Validate_WithBuyerNameExceeding100Characters_ShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateEggSaleCommand
        {
            Id = Guid.NewGuid(),
            Date = DateTime.UtcNow.Date,
            Quantity = 100,
            PricePerUnit = 5.00m,
            BuyerName = new string('A', 101)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BuyerName)
            .WithErrorMessage("Buyer name must not exceed 100 characters.");
    }

    #endregion

    #region Notes Validation Tests

    [Fact]
    public void Validate_WithNullNotes_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new UpdateEggSaleCommand
        {
            Id = Guid.NewGuid(),
            Date = DateTime.UtcNow.Date,
            Quantity = 100,
            PricePerUnit = 5.00m,
            Notes = null
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
        var command = new UpdateEggSaleCommand
        {
            Id = Guid.NewGuid(),
            Date = DateTime.UtcNow.Date,
            Quantity = 100,
            PricePerUnit = 5.00m,
            Notes = "Updated notes for this sale"
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
        var command = new UpdateEggSaleCommand
        {
            Id = Guid.NewGuid(),
            Date = DateTime.UtcNow.Date,
            Quantity = 100,
            PricePerUnit = 5.00m,
            Notes = new string('N', 501)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Notes)
            .WithErrorMessage("Notes must not exceed 500 characters.");
    }

    #endregion

    #region Full Command Validation Tests

    [Fact]
    public void Validate_WithFullyValidCommand_ShouldNotHaveAnyValidationErrors()
    {
        // Arrange
        var command = new UpdateEggSaleCommand
        {
            Id = Guid.NewGuid(),
            Date = DateTime.UtcNow.Date,
            Quantity = 200,
            PricePerUnit = 6.00m,
            BuyerName = "New Buyer",
            Notes = "Updated notes"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion
}
