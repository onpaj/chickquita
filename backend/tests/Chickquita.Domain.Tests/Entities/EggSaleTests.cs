using Chickquita.Domain.Common;
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
    private static readonly DateTime ValidDate = new DateTime(2026, 3, 15, 0, 0, 0, DateTimeKind.Utc);
    private const int ValidQuantity = 50;
    private const decimal ValidPricePerUnit = 5.50m;
    private const string ValidBuyerName = "John Smith";
    private const string ValidNotes = "Regular weekly sale";

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
    }

    [Fact]
    public void Create_WithoutOptionalFields_ShouldSucceed()
    {
        // Arrange & Act
        var result = EggSale.Create(
            _validTenantId,
            ValidDate,
            ValidQuantity,
            ValidPricePerUnit);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var sale = result.Value;
        sale.Should().NotBeNull();
        sale.BuyerName.Should().BeNull();
        sale.Notes.Should().BeNull();
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
    public void Create_ShouldNormalizeDateToUtcMidnight()
    {
        // Arrange
        var dateWithTime = new DateTime(2026, 3, 15, 14, 30, 45, DateTimeKind.Utc);

        // Act
        var result = EggSale.Create(
            _validTenantId,
            dateWithTime,
            ValidQuantity,
            ValidPricePerUnit);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var sale = result.Value;
        sale.Date.Hour.Should().Be(0);
        sale.Date.Minute.Should().Be(0);
        sale.Date.Second.Should().Be(0);
        sale.Date.Millisecond.Should().Be(0);
        sale.Date.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void Create_WithLocalDate_ShouldConvertToUtc()
    {
        // Arrange
        var localDate = new DateTime(2026, 3, 15, 0, 0, 0, DateTimeKind.Local);

        // Act
        var result = EggSale.Create(
            _validTenantId,
            localDate,
            ValidQuantity,
            ValidPricePerUnit);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Date.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void Create_WithUnspecifiedKindDate_ShouldTreatAsUtc()
    {
        // Arrange
        var unspecifiedDate = new DateTime(2026, 3, 15, 0, 0, 0, DateTimeKind.Unspecified);

        // Act
        var result = EggSale.Create(
            _validTenantId,
            unspecifiedDate,
            ValidQuantity,
            ValidPricePerUnit);

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
        var result = EggSale.Create(
            Guid.Empty,
            ValidDate,
            ValidQuantity,
            ValidPricePerUnit);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Tenant ID cannot be empty");
    }

    #endregion

    #region Validation Tests - Date

    [Fact]
    public void Create_WithDefaultDate_ShouldReturnFailure()
    {
        // Arrange & Act
        var result = EggSale.Create(
            _validTenantId,
            default,
            ValidQuantity,
            ValidPricePerUnit);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Sale date cannot be a default value");
    }

    #endregion

    #region Validation Tests - Quantity

    [Fact]
    public void Create_WithZeroQuantity_ShouldReturnFailure()
    {
        // Arrange & Act
        var result = EggSale.Create(
            _validTenantId,
            ValidDate,
            0,
            ValidPricePerUnit);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Quantity must be greater than zero");
    }

    [Fact]
    public void Create_WithNegativeQuantity_ShouldReturnFailure()
    {
        // Arrange & Act
        var result = EggSale.Create(
            _validTenantId,
            ValidDate,
            -1,
            ValidPricePerUnit);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Quantity must be greater than zero");
    }

    [Fact]
    public void Create_WithLargeQuantity_ShouldSucceed()
    {
        // Arrange & Act
        var result = EggSale.Create(
            _validTenantId,
            ValidDate,
            10000,
            ValidPricePerUnit);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Quantity.Should().Be(10000);
    }

    #endregion

    #region Validation Tests - Price Per Unit

    [Fact]
    public void Create_WithZeroPricePerUnit_ShouldReturnFailure()
    {
        // Arrange & Act
        var result = EggSale.Create(
            _validTenantId,
            ValidDate,
            ValidQuantity,
            0m);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Price per unit must be greater than zero");
    }

    [Fact]
    public void Create_WithNegativePricePerUnit_ShouldReturnFailure()
    {
        // Arrange & Act
        var result = EggSale.Create(
            _validTenantId,
            ValidDate,
            ValidQuantity,
            -0.01m);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Price per unit must be greater than zero");
    }

    [Fact]
    public void Create_WithSmallPositivePricePerUnit_ShouldSucceed()
    {
        // Arrange & Act
        var result = EggSale.Create(
            _validTenantId,
            ValidDate,
            ValidQuantity,
            0.0001m);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.PricePerUnit.Should().Be(0.0001m);
    }

    #endregion

    #region Validation Tests - Buyer Name

    [Fact]
    public void Create_WithNullBuyerName_ShouldSucceed()
    {
        // Arrange & Act
        var result = EggSale.Create(
            _validTenantId,
            ValidDate,
            ValidQuantity,
            ValidPricePerUnit,
            buyerName: null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.BuyerName.Should().BeNull();
    }

    [Fact]
    public void Create_WithBuyerNameExactly200Characters_ShouldSucceed()
    {
        // Arrange
        var buyerName = new string('A', 200);

        // Act
        var result = EggSale.Create(
            _validTenantId,
            ValidDate,
            ValidQuantity,
            ValidPricePerUnit,
            buyerName: buyerName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.BuyerName.Should().Be(buyerName);
        result.Value.BuyerName!.Length.Should().Be(200);
    }

    [Fact]
    public void Create_WithBuyerNameExceeding200Characters_ShouldReturnFailure()
    {
        // Arrange
        var longBuyerName = new string('A', 201);

        // Act
        var result = EggSale.Create(
            _validTenantId,
            ValidDate,
            ValidQuantity,
            ValidPricePerUnit,
            buyerName: longBuyerName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Buyer name cannot exceed 200 characters");
    }

    #endregion

    #region Validation Tests - Notes

    [Fact]
    public void Create_WithNullNotes_ShouldSucceed()
    {
        // Arrange & Act
        var result = EggSale.Create(
            _validTenantId,
            ValidDate,
            ValidQuantity,
            ValidPricePerUnit,
            notes: null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Notes.Should().BeNull();
    }

    [Fact]
    public void Create_WithNotesExactly1000Characters_ShouldSucceed()
    {
        // Arrange
        var notes = new string('A', 1000);

        // Act
        var result = EggSale.Create(
            _validTenantId,
            ValidDate,
            ValidQuantity,
            ValidPricePerUnit,
            notes: notes);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Notes.Should().Be(notes);
        result.Value.Notes!.Length.Should().Be(1000);
    }

    [Fact]
    public void Create_WithNotesExceeding1000Characters_ShouldReturnFailure()
    {
        // Arrange
        var longNotes = new string('A', 1001);

        // Act
        var result = EggSale.Create(
            _validTenantId,
            ValidDate,
            ValidQuantity,
            ValidPricePerUnit,
            notes: longNotes);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Notes cannot exceed 1000 characters");
    }

    #endregion

    #region Update Tests

    [Fact]
    public void Update_WithValidData_ShouldUpdateAllFields()
    {
        // Arrange
        var sale = EggSale.Create(
            _validTenantId,
            ValidDate,
            ValidQuantity,
            ValidPricePerUnit,
            ValidBuyerName,
            ValidNotes).Value;

        var originalTenantId = sale.TenantId;
        var originalCreatedAt = sale.CreatedAt;
        var newDate = ValidDate.AddDays(1);
        var newQuantity = 100;
        var newPricePerUnit = 6.00m;
        var newBuyerName = "Jane Doe";
        var newNotes = "Updated notes";

        // Act
        var updateResult = sale.Update(newDate, newQuantity, newPricePerUnit, newBuyerName, newNotes);

        // Assert
        updateResult.IsSuccess.Should().BeTrue();
        sale.Date.Should().Be(newDate);
        sale.Quantity.Should().Be(newQuantity);
        sale.PricePerUnit.Should().Be(newPricePerUnit);
        sale.BuyerName.Should().Be(newBuyerName);
        sale.Notes.Should().Be(newNotes);
        sale.TenantId.Should().Be(originalTenantId);
        sale.CreatedAt.Should().Be(originalCreatedAt);
    }

    [Fact]
    public void Update_WithNullOptionalFields_ShouldClearThem()
    {
        // Arrange
        var sale = EggSale.Create(
            _validTenantId,
            ValidDate,
            ValidQuantity,
            ValidPricePerUnit,
            ValidBuyerName,
            ValidNotes).Value;

        // Act
        var updateResult = sale.Update(ValidDate, ValidQuantity, ValidPricePerUnit, null, null);

        // Assert
        updateResult.IsSuccess.Should().BeTrue();
        sale.BuyerName.Should().BeNull();
        sale.Notes.Should().BeNull();
    }

    [Fact]
    public void Update_ShouldNotModifyTenantId()
    {
        // Arrange
        var sale = EggSale.Create(
            _validTenantId,
            ValidDate,
            ValidQuantity,
            ValidPricePerUnit).Value;

        // Act
        var updateResult = sale.Update(ValidDate, ValidQuantity, ValidPricePerUnit, null, null);

        // Assert
        updateResult.IsSuccess.Should().BeTrue();
        sale.TenantId.Should().Be(_validTenantId);
    }

    [Fact]
    public void Update_WithDefaultDate_ShouldReturnFailure()
    {
        // Arrange
        var sale = EggSale.Create(
            _validTenantId,
            ValidDate,
            ValidQuantity,
            ValidPricePerUnit).Value;

        // Act
        var updateResult = sale.Update(default, ValidQuantity, ValidPricePerUnit, null, null);

        // Assert
        updateResult.IsFailure.Should().BeTrue();
        updateResult.Error.Message.Should().Contain("Sale date cannot be a default value");
    }

    [Fact]
    public void Update_WithZeroQuantity_ShouldReturnFailure()
    {
        // Arrange
        var sale = EggSale.Create(
            _validTenantId,
            ValidDate,
            ValidQuantity,
            ValidPricePerUnit).Value;

        // Act
        var updateResult = sale.Update(ValidDate, 0, ValidPricePerUnit, null, null);

        // Assert
        updateResult.IsFailure.Should().BeTrue();
        updateResult.Error.Message.Should().Contain("Quantity must be greater than zero");
    }

    [Fact]
    public void Update_WithNegativeQuantity_ShouldReturnFailure()
    {
        // Arrange
        var sale = EggSale.Create(
            _validTenantId,
            ValidDate,
            ValidQuantity,
            ValidPricePerUnit).Value;

        // Act
        var updateResult = sale.Update(ValidDate, -5, ValidPricePerUnit, null, null);

        // Assert
        updateResult.IsFailure.Should().BeTrue();
        updateResult.Error.Message.Should().Contain("Quantity must be greater than zero");
    }

    [Fact]
    public void Update_WithZeroPricePerUnit_ShouldReturnFailure()
    {
        // Arrange
        var sale = EggSale.Create(
            _validTenantId,
            ValidDate,
            ValidQuantity,
            ValidPricePerUnit).Value;

        // Act
        var updateResult = sale.Update(ValidDate, ValidQuantity, 0m, null, null);

        // Assert
        updateResult.IsFailure.Should().BeTrue();
        updateResult.Error.Message.Should().Contain("Price per unit must be greater than zero");
    }

    [Fact]
    public void Update_WithNegativePricePerUnit_ShouldReturnFailure()
    {
        // Arrange
        var sale = EggSale.Create(
            _validTenantId,
            ValidDate,
            ValidQuantity,
            ValidPricePerUnit).Value;

        // Act
        var updateResult = sale.Update(ValidDate, ValidQuantity, -1m, null, null);

        // Assert
        updateResult.IsFailure.Should().BeTrue();
        updateResult.Error.Message.Should().Contain("Price per unit must be greater than zero");
    }

    [Fact]
    public void Update_WithBuyerNameExceeding200Characters_ShouldReturnFailure()
    {
        // Arrange
        var sale = EggSale.Create(
            _validTenantId,
            ValidDate,
            ValidQuantity,
            ValidPricePerUnit).Value;

        var longBuyerName = new string('B', 201);

        // Act
        var updateResult = sale.Update(ValidDate, ValidQuantity, ValidPricePerUnit, longBuyerName, null);

        // Assert
        updateResult.IsFailure.Should().BeTrue();
        updateResult.Error.Message.Should().Contain("Buyer name cannot exceed 200 characters");
    }

    [Fact]
    public void Update_WithNotesExceeding1000Characters_ShouldReturnFailure()
    {
        // Arrange
        var sale = EggSale.Create(
            _validTenantId,
            ValidDate,
            ValidQuantity,
            ValidPricePerUnit).Value;

        var longNotes = new string('B', 1001);

        // Act
        var updateResult = sale.Update(ValidDate, ValidQuantity, ValidPricePerUnit, null, longNotes);

        // Assert
        updateResult.IsFailure.Should().BeTrue();
        updateResult.Error.Message.Should().Contain("Notes cannot exceed 1000 characters");
    }

    [Fact]
    public void Update_ShouldNormalizeDateToUtcMidnight()
    {
        // Arrange
        var sale = EggSale.Create(
            _validTenantId,
            ValidDate,
            ValidQuantity,
            ValidPricePerUnit).Value;

        var dateWithTime = new DateTime(2026, 3, 16, 18, 45, 30, DateTimeKind.Utc);

        // Act
        var updateResult = sale.Update(dateWithTime, ValidQuantity, ValidPricePerUnit, null, null);

        // Assert
        updateResult.IsSuccess.Should().BeTrue();
        sale.Date.Hour.Should().Be(0);
        sale.Date.Minute.Should().Be(0);
        sale.Date.Second.Should().Be(0);
        sale.Date.Kind.Should().Be(DateTimeKind.Utc);
    }

    #endregion
}
