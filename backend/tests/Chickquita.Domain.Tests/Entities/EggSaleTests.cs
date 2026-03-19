using Chickquita.Domain.Entities;
using FluentAssertions;

namespace Chickquita.Domain.Tests.Entities;

/// <summary>
/// Unit tests for the EggSale domain entity.
/// Tests focus on domain logic, invariants, and validation rules.
/// </summary>
public class EggSaleTests
{
    private readonly Guid _validTenantId = Guid.NewGuid();
    private static readonly DateTime ValidDate = new DateTime(2024, 6, 15, 0, 0, 0, DateTimeKind.Utc);
    private const int ValidQuantity = 100;
    private const decimal ValidPricePerUnit = 0.25m;
    private const string ValidBuyerName = "Local Market";
    private const string ValidNotes = "Weekly sale";

    #region Create Tests

    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        // Arrange & Act
        var result = EggSale.Create(
            _validTenantId,
            ValidDate,
            ValidQuantity,
            ValidPricePerUnit,
            ValidBuyerName,
            ValidNotes);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var sale = result.Value;
        sale.Should().NotBeNull();
        sale.Id.Should().NotBeEmpty();
        sale.TenantId.Should().Be(_validTenantId);
        sale.Date.Should().Be(ValidDate);
        sale.Quantity.Should().Be(ValidQuantity);
        sale.PricePerUnit.Should().Be(ValidPricePerUnit);
        sale.BuyerName.Should().Be(ValidBuyerName);
        sale.Notes.Should().Be(ValidNotes);
        sale.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        sale.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        sale.CreatedAt.Should().Be(sale.UpdatedAt);
    }

    [Fact]
    public void Create_WithoutOptionalFields_ShouldSucceed()
    {
        // Arrange & Act
        var result = EggSale.Create(_validTenantId, ValidDate, ValidQuantity, ValidPricePerUnit);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var sale = result.Value;
        sale.Should().NotBeNull();
        sale.BuyerName.Should().BeNull();
        sale.Notes.Should().BeNull();
    }

    [Fact]
    public void Create_WithZeroPricePerUnit_ShouldSucceed()
    {
        // Arrange & Act
        var result = EggSale.Create(_validTenantId, ValidDate, ValidQuantity, 0m);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.PricePerUnit.Should().Be(0m);
    }

    [Fact]
    public void Create_ShouldGenerateUniqueIds()
    {
        // Arrange & Act
        var sale1 = EggSale.Create(_validTenantId, ValidDate, ValidQuantity, ValidPricePerUnit).Value;
        var sale2 = EggSale.Create(_validTenantId, ValidDate, ValidQuantity, ValidPricePerUnit).Value;

        // Assert
        sale1.Id.Should().NotBe(sale2.Id);
    }

    [Fact]
    public void Create_ShouldNormalizeDateToMidnight()
    {
        // Arrange
        var dateWithTime = new DateTime(2024, 6, 15, 14, 30, 0, DateTimeKind.Utc);

        // Act
        var result = EggSale.Create(_validTenantId, dateWithTime, ValidQuantity, ValidPricePerUnit);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var sale = result.Value;
        sale.Date.Hour.Should().Be(0);
        sale.Date.Minute.Should().Be(0);
        sale.Date.Second.Should().Be(0);
        sale.Date.Millisecond.Should().Be(0);
    }

    [Fact]
    public void Create_WithLocalDate_ShouldConvertToUtc()
    {
        // Arrange
        var localDate = new DateTime(2024, 6, 15, 0, 0, 0, DateTimeKind.Local);

        // Act
        var result = EggSale.Create(_validTenantId, localDate, ValidQuantity, ValidPricePerUnit);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Date.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void Create_WithUnspecifiedDateKind_ShouldTreatAsUtc()
    {
        // Arrange
        var unspecifiedDate = new DateTime(2024, 6, 15, 0, 0, 0, DateTimeKind.Unspecified);

        // Act
        var result = EggSale.Create(_validTenantId, unspecifiedDate, ValidQuantity, ValidPricePerUnit);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Date.Kind.Should().Be(DateTimeKind.Utc);
    }

    #endregion

    #region Validation Tests - Tenant ID

    [Fact]
    public void Create_WithEmptyTenantId_ShouldReturnFailure()
    {
        // Arrange & Act
        var result = EggSale.Create(Guid.Empty, ValidDate, ValidQuantity, ValidPricePerUnit);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Tenant ID cannot be empty");
    }

    #endregion

    #region Validation Tests - Quantity

    [Fact]
    public void Create_WithZeroQuantity_ShouldReturnFailure()
    {
        // Arrange & Act
        var result = EggSale.Create(_validTenantId, ValidDate, 0, ValidPricePerUnit);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Quantity must be greater than zero");
    }

    [Fact]
    public void Create_WithNegativeQuantity_ShouldReturnFailure()
    {
        // Arrange & Act
        var result = EggSale.Create(_validTenantId, ValidDate, -10, ValidPricePerUnit);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Quantity must be greater than zero");
    }

    [Fact]
    public void Create_WithLargeQuantity_ShouldSucceed()
    {
        // Arrange & Act
        var result = EggSale.Create(_validTenantId, ValidDate, 10000, ValidPricePerUnit);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Quantity.Should().Be(10000);
    }

    #endregion

    #region Validation Tests - PricePerUnit

    [Fact]
    public void Create_WithNegativePricePerUnit_ShouldReturnFailure()
    {
        // Arrange & Act
        var result = EggSale.Create(_validTenantId, ValidDate, ValidQuantity, -0.01m);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Price per unit cannot be negative");
    }

    [Fact]
    public void Create_WithLargePricePerUnit_ShouldSucceed()
    {
        // Arrange & Act
        var result = EggSale.Create(_validTenantId, ValidDate, ValidQuantity, 999.99m);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.PricePerUnit.Should().Be(999.99m);
    }

    #endregion

    #region Validation Tests - BuyerName

    [Fact]
    public void Create_WithBuyerNameExceeding100Characters_ShouldReturnFailure()
    {
        // Arrange
        var longName = new string('A', 101);

        // Act
        var result = EggSale.Create(_validTenantId, ValidDate, ValidQuantity, ValidPricePerUnit, longName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Buyer name cannot exceed 100 characters");
    }

    [Fact]
    public void Create_WithBuyerNameExactly100Characters_ShouldSucceed()
    {
        // Arrange
        var name = new string('A', 100);

        // Act
        var result = EggSale.Create(_validTenantId, ValidDate, ValidQuantity, ValidPricePerUnit, name);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.BuyerName.Should().Be(name);
    }

    #endregion

    #region Validation Tests - Notes

    [Fact]
    public void Create_WithNotesExceeding500Characters_ShouldReturnFailure()
    {
        // Arrange
        var longNotes = new string('A', 501);

        // Act
        var result = EggSale.Create(_validTenantId, ValidDate, ValidQuantity, ValidPricePerUnit, null, longNotes);

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
        var result = EggSale.Create(_validTenantId, ValidDate, ValidQuantity, ValidPricePerUnit, null, notes);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Notes.Should().Be(notes);
    }

    #endregion

    #region Update Tests

    [Fact]
    public void Update_WithValidData_ShouldUpdateAllFields()
    {
        // Arrange
        var sale = EggSale.Create(_validTenantId, ValidDate, ValidQuantity, ValidPricePerUnit).Value;
        var originalCreatedAt = sale.CreatedAt;
        var originalUpdatedAt = sale.UpdatedAt;
        Thread.Sleep(10);

        var newDate = ValidDate.AddDays(1);
        var newQuantity = 200;
        var newPrice = 0.30m;
        var newBuyerName = "New Buyer";
        var newNotes = "Updated notes";

        // Act
        var updateResult = sale.Update(newDate, newQuantity, newPrice, newBuyerName, newNotes);

        // Assert
        updateResult.IsSuccess.Should().BeTrue();
        sale.Date.Should().Be(newDate);
        sale.Quantity.Should().Be(newQuantity);
        sale.PricePerUnit.Should().Be(newPrice);
        sale.BuyerName.Should().Be(newBuyerName);
        sale.Notes.Should().Be(newNotes);
        sale.CreatedAt.Should().Be(originalCreatedAt);
        sale.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void Update_WithoutOptionalFields_ShouldClearOptionalFields()
    {
        // Arrange
        var sale = EggSale.Create(_validTenantId, ValidDate, ValidQuantity, ValidPricePerUnit, ValidBuyerName, ValidNotes).Value;

        // Act
        var updateResult = sale.Update(ValidDate, ValidQuantity, ValidPricePerUnit);

        // Assert
        updateResult.IsSuccess.Should().BeTrue();
        sale.BuyerName.Should().BeNull();
        sale.Notes.Should().BeNull();
    }

    [Fact]
    public void Update_WithZeroQuantity_ShouldReturnFailure()
    {
        // Arrange
        var sale = EggSale.Create(_validTenantId, ValidDate, ValidQuantity, ValidPricePerUnit).Value;

        // Act
        var updateResult = sale.Update(ValidDate, 0, ValidPricePerUnit);

        // Assert
        updateResult.IsFailure.Should().BeTrue();
        updateResult.Error.Message.Should().Contain("Quantity must be greater than zero");
    }

    [Fact]
    public void Update_WithNegativePricePerUnit_ShouldReturnFailure()
    {
        // Arrange
        var sale = EggSale.Create(_validTenantId, ValidDate, ValidQuantity, ValidPricePerUnit).Value;

        // Act
        var updateResult = sale.Update(ValidDate, ValidQuantity, -1m);

        // Assert
        updateResult.IsFailure.Should().BeTrue();
        updateResult.Error.Message.Should().Contain("Price per unit cannot be negative");
    }

    [Fact]
    public void Update_WithBuyerNameExceeding100Characters_ShouldReturnFailure()
    {
        // Arrange
        var sale = EggSale.Create(_validTenantId, ValidDate, ValidQuantity, ValidPricePerUnit).Value;
        var longName = new string('B', 101);

        // Act
        var updateResult = sale.Update(ValidDate, ValidQuantity, ValidPricePerUnit, longName);

        // Assert
        updateResult.IsFailure.Should().BeTrue();
        updateResult.Error.Message.Should().Contain("Buyer name cannot exceed 100 characters");
    }

    [Fact]
    public void Update_WithNotesExceeding500Characters_ShouldReturnFailure()
    {
        // Arrange
        var sale = EggSale.Create(_validTenantId, ValidDate, ValidQuantity, ValidPricePerUnit).Value;
        var longNotes = new string('B', 501);

        // Act
        var updateResult = sale.Update(ValidDate, ValidQuantity, ValidPricePerUnit, null, longNotes);

        // Assert
        updateResult.IsFailure.Should().BeTrue();
        updateResult.Error.Message.Should().Contain("Notes cannot exceed 500 characters");
    }

    [Fact]
    public void Update_ShouldNotModifyTenantId()
    {
        // Arrange
        var sale = EggSale.Create(_validTenantId, ValidDate, ValidQuantity, ValidPricePerUnit).Value;

        // Act
        var updateResult = sale.Update(ValidDate, ValidQuantity, ValidPricePerUnit);

        // Assert
        updateResult.IsSuccess.Should().BeTrue();
        sale.TenantId.Should().Be(_validTenantId);
    }

    #endregion
}
