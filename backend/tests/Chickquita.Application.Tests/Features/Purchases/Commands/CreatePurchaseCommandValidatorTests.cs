using Chickquita.Application.Features.Purchases.Commands.Create;
using Chickquita.Domain.Entities;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Chickquita.Application.Tests.Features.Purchases.Commands;

/// <summary>
/// Unit tests for CreatePurchaseCommandValidator.
/// Tests all validation rules for the CreatePurchaseCommand.
/// </summary>
public class CreatePurchaseCommandValidatorTests
{
    private readonly CreatePurchaseCommandValidator _validator;

    public CreatePurchaseCommandValidatorTests()
    {
        _validator = new CreatePurchaseCommandValidator();
    }

    #region Name Validation Tests

    [Fact]
    public void Validate_WithValidName_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new CreatePurchaseCommand
        {
            Name = "Chicken Feed",
            Type = PurchaseType.Feed,
            Amount = 250.50m,
            Quantity = 25m,
            Unit = QuantityUnit.Kg,
            PurchaseDate = DateTime.UtcNow.Date
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_WithEmptyName_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreatePurchaseCommand
        {
            Name = string.Empty,
            Type = PurchaseType.Feed,
            Amount = 250.50m,
            Quantity = 25m,
            Unit = QuantityUnit.Kg,
            PurchaseDate = DateTime.UtcNow.Date
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Purchase name is required.");
    }

    [Fact]
    public void Validate_WithNameExceeding100Characters_ShouldHaveValidationError()
    {
        // Arrange
        var longName = new string('A', 101);
        var command = new CreatePurchaseCommand
        {
            Name = longName,
            Type = PurchaseType.Feed,
            Amount = 250.50m,
            Quantity = 25m,
            Unit = QuantityUnit.Kg,
            PurchaseDate = DateTime.UtcNow.Date
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Purchase name must not exceed 100 characters.");
    }

    #endregion

    #region Type Validation Tests

    [Theory]
    [InlineData(PurchaseType.Feed)]
    [InlineData(PurchaseType.Vitamins)]
    [InlineData(PurchaseType.Bedding)]
    [InlineData(PurchaseType.Toys)]
    [InlineData(PurchaseType.Veterinary)]
    [InlineData(PurchaseType.Other)]
    public void Validate_WithValidType_ShouldNotHaveValidationError(PurchaseType type)
    {
        // Arrange
        var command = new CreatePurchaseCommand
        {
            Name = "Test Purchase",
            Type = type,
            Amount = 100m,
            Quantity = 10m,
            Unit = QuantityUnit.Kg,
            PurchaseDate = DateTime.UtcNow.Date
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Type);
    }

    [Fact]
    public void Validate_WithInvalidType_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreatePurchaseCommand
        {
            Name = "Test Purchase",
            Type = (PurchaseType)999,
            Amount = 100m,
            Quantity = 10m,
            Unit = QuantityUnit.Kg,
            PurchaseDate = DateTime.UtcNow.Date
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Type)
            .WithErrorMessage("Purchase type must be a valid value.");
    }

    #endregion

    #region Amount Validation Tests

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    [InlineData(9999.99)]
    public void Validate_WithValidAmount_ShouldNotHaveValidationError(decimal amount)
    {
        // Arrange
        var command = new CreatePurchaseCommand
        {
            Name = "Test Purchase",
            Type = PurchaseType.Feed,
            Amount = amount,
            Quantity = 10m,
            Unit = QuantityUnit.Kg,
            PurchaseDate = DateTime.UtcNow.Date
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void Validate_WithNegativeAmount_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreatePurchaseCommand
        {
            Name = "Test Purchase",
            Type = PurchaseType.Feed,
            Amount = -100m,
            Quantity = 10m,
            Unit = QuantityUnit.Kg,
            PurchaseDate = DateTime.UtcNow.Date
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Amount)
            .WithErrorMessage("Amount must be greater than or equal to zero.");
    }

    #endregion

    #region Quantity Validation Tests

    [Theory]
    [InlineData(0.01)]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(9999.99)]
    public void Validate_WithValidQuantity_ShouldNotHaveValidationError(decimal quantity)
    {
        // Arrange
        var command = new CreatePurchaseCommand
        {
            Name = "Test Purchase",
            Type = PurchaseType.Feed,
            Amount = 100m,
            Quantity = quantity,
            Unit = QuantityUnit.Kg,
            PurchaseDate = DateTime.UtcNow.Date
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
    public void Validate_WithZeroOrNegativeQuantity_ShouldHaveValidationError(decimal quantity)
    {
        // Arrange
        var command = new CreatePurchaseCommand
        {
            Name = "Test Purchase",
            Type = PurchaseType.Feed,
            Amount = 100m,
            Quantity = quantity,
            Unit = QuantityUnit.Kg,
            PurchaseDate = DateTime.UtcNow.Date
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Quantity)
            .WithErrorMessage("Quantity must be greater than zero.");
    }

    #endregion

    #region Unit Validation Tests

    [Theory]
    [InlineData(QuantityUnit.Kg)]
    [InlineData(QuantityUnit.Pcs)]
    [InlineData(QuantityUnit.L)]
    [InlineData(QuantityUnit.Package)]
    [InlineData(QuantityUnit.Other)]
    public void Validate_WithValidUnit_ShouldNotHaveValidationError(QuantityUnit unit)
    {
        // Arrange
        var command = new CreatePurchaseCommand
        {
            Name = "Test Purchase",
            Type = PurchaseType.Feed,
            Amount = 100m,
            Quantity = 10m,
            Unit = unit,
            PurchaseDate = DateTime.UtcNow.Date
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Unit);
    }

    [Fact]
    public void Validate_WithInvalidUnit_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreatePurchaseCommand
        {
            Name = "Test Purchase",
            Type = PurchaseType.Feed,
            Amount = 100m,
            Quantity = 10m,
            Unit = (QuantityUnit)999,
            PurchaseDate = DateTime.UtcNow.Date
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Unit)
            .WithErrorMessage("Quantity unit must be a valid value.");
    }

    #endregion

    #region PurchaseDate Validation Tests

    [Fact]
    public void Validate_WithTodaysPurchaseDate_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new CreatePurchaseCommand
        {
            Name = "Test Purchase",
            Type = PurchaseType.Feed,
            Amount = 100m,
            Quantity = 10m,
            Unit = QuantityUnit.Kg,
            PurchaseDate = DateTime.UtcNow.Date
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PurchaseDate);
    }

    [Fact]
    public void Validate_WithPastPurchaseDate_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new CreatePurchaseCommand
        {
            Name = "Test Purchase",
            Type = PurchaseType.Feed,
            Amount = 100m,
            Quantity = 10m,
            Unit = QuantityUnit.Kg,
            PurchaseDate = DateTime.UtcNow.Date.AddDays(-30)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PurchaseDate);
    }

    [Fact]
    public void Validate_WithFuturePurchaseDate_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreatePurchaseCommand
        {
            Name = "Test Purchase",
            Type = PurchaseType.Feed,
            Amount = 100m,
            Quantity = 10m,
            Unit = QuantityUnit.Kg,
            PurchaseDate = DateTime.UtcNow.Date.AddDays(2)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PurchaseDate)
            .WithErrorMessage("Purchase date cannot be in the future.");
    }

    #endregion

    #region ConsumedDate Validation Tests

    [Fact]
    public void Validate_WithNullConsumedDate_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new CreatePurchaseCommand
        {
            Name = "Test Purchase",
            Type = PurchaseType.Feed,
            Amount = 100m,
            Quantity = 10m,
            Unit = QuantityUnit.Kg,
            PurchaseDate = DateTime.UtcNow.Date,
            ConsumedDate = null
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ConsumedDate);
    }

    [Fact]
    public void Validate_WithConsumedDateAfterPurchaseDate_ShouldNotHaveValidationError()
    {
        // Arrange
        var purchaseDate = DateTime.UtcNow.Date.AddDays(-5);
        var consumedDate = DateTime.UtcNow.Date;
        var command = new CreatePurchaseCommand
        {
            Name = "Test Purchase",
            Type = PurchaseType.Feed,
            Amount = 100m,
            Quantity = 10m,
            Unit = QuantityUnit.Kg,
            PurchaseDate = purchaseDate,
            ConsumedDate = consumedDate
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ConsumedDate);
    }

    [Fact]
    public void Validate_WithConsumedDateSameAsPurchaseDate_ShouldNotHaveValidationError()
    {
        // Arrange
        var date = DateTime.UtcNow.Date;
        var command = new CreatePurchaseCommand
        {
            Name = "Test Purchase",
            Type = PurchaseType.Feed,
            Amount = 100m,
            Quantity = 10m,
            Unit = QuantityUnit.Kg,
            PurchaseDate = date,
            ConsumedDate = date
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ConsumedDate);
    }

    [Fact]
    public void Validate_WithConsumedDateBeforePurchaseDate_ShouldHaveValidationError()
    {
        // Arrange
        var purchaseDate = DateTime.UtcNow.Date;
        var consumedDate = purchaseDate.AddDays(-1);
        var command = new CreatePurchaseCommand
        {
            Name = "Test Purchase",
            Type = PurchaseType.Feed,
            Amount = 100m,
            Quantity = 10m,
            Unit = QuantityUnit.Kg,
            PurchaseDate = purchaseDate,
            ConsumedDate = consumedDate
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ConsumedDate)
            .WithErrorMessage("Consumed date cannot be before purchase date.");
    }

    #endregion

    #region Notes Validation Tests

    [Fact]
    public void Validate_WithNullNotes_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new CreatePurchaseCommand
        {
            Name = "Test Purchase",
            Type = PurchaseType.Feed,
            Amount = 100m,
            Quantity = 10m,
            Unit = QuantityUnit.Kg,
            PurchaseDate = DateTime.UtcNow.Date,
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
        var command = new CreatePurchaseCommand
        {
            Name = "Test Purchase",
            Type = PurchaseType.Feed,
            Amount = 100m,
            Quantity = 10m,
            Unit = QuantityUnit.Kg,
            PurchaseDate = DateTime.UtcNow.Date,
            Notes = "High quality organic feed"
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
        var longNotes = new string('N', 501);
        var command = new CreatePurchaseCommand
        {
            Name = "Test Purchase",
            Type = PurchaseType.Feed,
            Amount = 100m,
            Quantity = 10m,
            Unit = QuantityUnit.Kg,
            PurchaseDate = DateTime.UtcNow.Date,
            Notes = longNotes
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Notes)
            .WithErrorMessage("Notes must not exceed 500 characters.");
    }

    #endregion
}
