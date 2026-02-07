using Chickquita.Domain.Entities;
using FluentAssertions;

namespace Chickquita.Domain.Tests.Entities;

/// <summary>
/// Unit tests for the Purchase domain entity.
/// Tests focus on domain logic, invariants, and validation rules.
/// </summary>
public class PurchaseTests
{
    private readonly Guid _validTenantId = Guid.NewGuid();
    private readonly Guid _validCoopId = Guid.NewGuid();
    private static readonly DateTime ValidPurchaseDate = DateTime.UtcNow.AddDays(-5).Date;
    private const string ValidName = "Premium Chicken Feed";
    private const PurchaseType ValidType = PurchaseType.Feed;
    private const decimal ValidAmount = 25.50m;
    private const decimal ValidQuantity = 10.0m;
    private const QuantityUnit ValidUnit = QuantityUnit.Kg;
    private const string ValidNotes = "Good quality feed";

    #region Create Tests

    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        // Arrange & Act
        var purchase = Purchase.Create(
            _validTenantId,
            ValidName,
            ValidType,
            ValidAmount,
            ValidQuantity,
            ValidUnit,
            ValidPurchaseDate,
            _validCoopId,
            null,
            ValidNotes);

        // Assert
        purchase.Should().NotBeNull();
        purchase.Id.Should().NotBeEmpty();
        purchase.TenantId.Should().Be(_validTenantId);
        purchase.CoopId.Should().Be(_validCoopId);
        purchase.Name.Should().Be(ValidName);
        purchase.Type.Should().Be(ValidType);
        purchase.Amount.Should().Be(ValidAmount);
        purchase.Quantity.Should().Be(ValidQuantity);
        purchase.Unit.Should().Be(ValidUnit);
        purchase.PurchaseDate.Should().Be(ValidPurchaseDate);
        purchase.ConsumedDate.Should().BeNull();
        purchase.Notes.Should().Be(ValidNotes);
        purchase.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        purchase.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        purchase.CreatedAt.Should().Be(purchase.UpdatedAt);
    }

    [Fact]
    public void Create_WithoutOptionalFields_ShouldSucceed()
    {
        // Arrange & Act
        var purchase = Purchase.Create(
            _validTenantId,
            ValidName,
            ValidType,
            ValidAmount,
            ValidQuantity,
            ValidUnit,
            ValidPurchaseDate);

        // Assert
        purchase.Should().NotBeNull();
        purchase.CoopId.Should().BeNull();
        purchase.ConsumedDate.Should().BeNull();
        purchase.Notes.Should().BeNull();
    }

    [Fact]
    public void Create_WithZeroAmount_ShouldSucceed()
    {
        // Arrange & Act
        var purchase = Purchase.Create(
            _validTenantId,
            ValidName,
            ValidType,
            0m,
            ValidQuantity,
            ValidUnit,
            ValidPurchaseDate);

        // Assert
        purchase.Should().NotBeNull();
        purchase.Amount.Should().Be(0m);
    }

    [Fact]
    public void Create_WithConsumedDate_ShouldSucceed()
    {
        // Arrange
        var consumedDate = ValidPurchaseDate.AddDays(2);

        // Act
        var purchase = Purchase.Create(
            _validTenantId,
            ValidName,
            ValidType,
            ValidAmount,
            ValidQuantity,
            ValidUnit,
            ValidPurchaseDate,
            null,
            consumedDate);

        // Assert
        purchase.Should().NotBeNull();
        purchase.ConsumedDate.Should().Be(consumedDate);
    }

    [Fact]
    public void Create_ShouldGenerateUniqueIds()
    {
        // Arrange & Act
        var purchase1 = Purchase.Create(_validTenantId, ValidName, ValidType, ValidAmount, ValidQuantity, ValidUnit, ValidPurchaseDate);
        var purchase2 = Purchase.Create(_validTenantId, ValidName, ValidType, ValidAmount, ValidQuantity, ValidUnit, ValidPurchaseDate);

        // Assert
        purchase1.Id.Should().NotBe(purchase2.Id);
    }

    [Fact]
    public void Create_ShouldNormalizePurchaseDateToMidnight()
    {
        // Arrange
        var dateWithTime = DateTime.UtcNow.AddDays(-5).AddHours(14).AddMinutes(30);

        // Act
        var purchase = Purchase.Create(
            _validTenantId,
            ValidName,
            ValidType,
            ValidAmount,
            ValidQuantity,
            ValidUnit,
            dateWithTime);

        // Assert
        purchase.PurchaseDate.Hour.Should().Be(0);
        purchase.PurchaseDate.Minute.Should().Be(0);
        purchase.PurchaseDate.Second.Should().Be(0);
        purchase.PurchaseDate.Millisecond.Should().Be(0);
    }

    [Fact]
    public void Create_ShouldNormalizeConsumedDateToMidnight()
    {
        // Arrange
        var dateWithTime = DateTime.UtcNow.AddDays(-3).AddHours(18).AddMinutes(45);

        // Act
        var purchase = Purchase.Create(
            _validTenantId,
            ValidName,
            ValidType,
            ValidAmount,
            ValidQuantity,
            ValidUnit,
            ValidPurchaseDate,
            null,
            dateWithTime);

        // Assert
        purchase.ConsumedDate!.Value.Hour.Should().Be(0);
        purchase.ConsumedDate!.Value.Minute.Should().Be(0);
        purchase.ConsumedDate!.Value.Second.Should().Be(0);
        purchase.ConsumedDate!.Value.Millisecond.Should().Be(0);
    }

    [Theory]
    [InlineData(PurchaseType.Feed)]
    [InlineData(PurchaseType.Vitamins)]
    [InlineData(PurchaseType.Bedding)]
    [InlineData(PurchaseType.Toys)]
    [InlineData(PurchaseType.Veterinary)]
    [InlineData(PurchaseType.Other)]
    public void Create_WithAllPurchaseTypes_ShouldSucceed(PurchaseType type)
    {
        // Arrange & Act
        var purchase = Purchase.Create(
            _validTenantId,
            ValidName,
            type,
            ValidAmount,
            ValidQuantity,
            ValidUnit,
            ValidPurchaseDate);

        // Assert
        purchase.Should().NotBeNull();
        purchase.Type.Should().Be(type);
    }

    [Theory]
    [InlineData(QuantityUnit.Kg)]
    [InlineData(QuantityUnit.Pcs)]
    [InlineData(QuantityUnit.L)]
    [InlineData(QuantityUnit.Package)]
    [InlineData(QuantityUnit.Other)]
    public void Create_WithAllQuantityUnits_ShouldSucceed(QuantityUnit unit)
    {
        // Arrange & Act
        var purchase = Purchase.Create(
            _validTenantId,
            ValidName,
            ValidType,
            ValidAmount,
            ValidQuantity,
            unit,
            ValidPurchaseDate);

        // Assert
        purchase.Should().NotBeNull();
        purchase.Unit.Should().Be(unit);
    }

    #endregion

    #region Validation Tests - Tenant ID

    [Fact]
    public void Create_WithEmptyTenantId_ShouldThrowArgumentException()
    {
        // Arrange
        var emptyTenantId = Guid.Empty;

        // Act
        var act = () => Purchase.Create(
            emptyTenantId,
            ValidName,
            ValidType,
            ValidAmount,
            ValidQuantity,
            ValidUnit,
            ValidPurchaseDate);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Tenant ID cannot be empty.*")
            .And.ParamName.Should().Be("tenantId");
    }

    #endregion

    #region Validation Tests - Name

    [Fact]
    public void Create_WithEmptyName_ShouldThrowArgumentException()
    {
        // Arrange & Act
        var act = () => Purchase.Create(
            _validTenantId,
            string.Empty,
            ValidType,
            ValidAmount,
            ValidQuantity,
            ValidUnit,
            ValidPurchaseDate);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Purchase name cannot be empty.*")
            .And.ParamName.Should().Be("name");
    }

    [Fact]
    public void Create_WithWhitespaceName_ShouldThrowArgumentException()
    {
        // Arrange & Act
        var act = () => Purchase.Create(
            _validTenantId,
            "   ",
            ValidType,
            ValidAmount,
            ValidQuantity,
            ValidUnit,
            ValidPurchaseDate);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Purchase name cannot be empty.*")
            .And.ParamName.Should().Be("name");
    }

    [Fact]
    public void Create_WithNameExceeding100Characters_ShouldThrowArgumentException()
    {
        // Arrange
        var longName = new string('A', 101);

        // Act
        var act = () => Purchase.Create(
            _validTenantId,
            longName,
            ValidType,
            ValidAmount,
            ValidQuantity,
            ValidUnit,
            ValidPurchaseDate);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Purchase name cannot exceed 100 characters.*")
            .And.ParamName.Should().Be("name");
    }

    [Fact]
    public void Create_WithNameExactly100Characters_ShouldSucceed()
    {
        // Arrange
        var name = new string('A', 100);

        // Act
        var purchase = Purchase.Create(
            _validTenantId,
            name,
            ValidType,
            ValidAmount,
            ValidQuantity,
            ValidUnit,
            ValidPurchaseDate);

        // Assert
        purchase.Should().NotBeNull();
        purchase.Name.Should().Be(name);
        purchase.Name.Length.Should().Be(100);
    }

    #endregion

    #region Validation Tests - Amount

    [Fact]
    public void Create_WithNegativeAmount_ShouldThrowArgumentException()
    {
        // Arrange & Act
        var act = () => Purchase.Create(
            _validTenantId,
            ValidName,
            ValidType,
            -0.01m,
            ValidQuantity,
            ValidUnit,
            ValidPurchaseDate);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Amount cannot be negative.*")
            .And.ParamName.Should().Be("amount");
    }

    [Fact]
    public void Create_WithLargeAmount_ShouldSucceed()
    {
        // Arrange & Act
        var purchase = Purchase.Create(
            _validTenantId,
            ValidName,
            ValidType,
            999999.99m,
            ValidQuantity,
            ValidUnit,
            ValidPurchaseDate);

        // Assert
        purchase.Should().NotBeNull();
        purchase.Amount.Should().Be(999999.99m);
    }

    #endregion

    #region Validation Tests - Quantity

    [Fact]
    public void Create_WithZeroQuantity_ShouldThrowArgumentException()
    {
        // Arrange & Act
        var act = () => Purchase.Create(
            _validTenantId,
            ValidName,
            ValidType,
            ValidAmount,
            0m,
            ValidUnit,
            ValidPurchaseDate);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Quantity must be greater than zero.*")
            .And.ParamName.Should().Be("quantity");
    }

    [Fact]
    public void Create_WithNegativeQuantity_ShouldThrowArgumentException()
    {
        // Arrange & Act
        var act = () => Purchase.Create(
            _validTenantId,
            ValidName,
            ValidType,
            ValidAmount,
            -5m,
            ValidUnit,
            ValidPurchaseDate);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Quantity must be greater than zero.*")
            .And.ParamName.Should().Be("quantity");
    }

    [Fact]
    public void Create_WithDecimalQuantity_ShouldSucceed()
    {
        // Arrange & Act
        var purchase = Purchase.Create(
            _validTenantId,
            ValidName,
            ValidType,
            ValidAmount,
            2.5m,
            ValidUnit,
            ValidPurchaseDate);

        // Assert
        purchase.Should().NotBeNull();
        purchase.Quantity.Should().Be(2.5m);
    }

    #endregion

    #region Validation Tests - Purchase Date

    [Fact]
    public void Create_WithLocalTime_ShouldConvertToUtc()
    {
        // Arrange
        var localDate = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Local);

        // Act
        var purchase = Purchase.Create(
            _validTenantId,
            ValidName,
            ValidType,
            ValidAmount,
            ValidQuantity,
            ValidUnit,
            localDate);

        // Assert
        purchase.PurchaseDate.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void Create_WithUnspecifiedKind_ShouldConvertToUtc()
    {
        // Arrange
        var unspecifiedDate = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Unspecified);

        // Act
        var purchase = Purchase.Create(
            _validTenantId,
            ValidName,
            ValidType,
            ValidAmount,
            ValidQuantity,
            ValidUnit,
            unspecifiedDate);

        // Assert
        purchase.PurchaseDate.Kind.Should().Be(DateTimeKind.Utc);
    }

    #endregion

    #region Validation Tests - Consumed Date

    [Fact]
    public void Create_WithConsumedDateBeforePurchaseDate_ShouldThrowArgumentException()
    {
        // Arrange
        var consumedDate = ValidPurchaseDate.AddDays(-1);

        // Act
        var act = () => Purchase.Create(
            _validTenantId,
            ValidName,
            ValidType,
            ValidAmount,
            ValidQuantity,
            ValidUnit,
            ValidPurchaseDate,
            null,
            consumedDate);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Consumed date cannot be before purchase date.*")
            .And.ParamName.Should().Be("consumedDate");
    }

    [Fact]
    public void Create_WithConsumedDateEqualToPurchaseDate_ShouldSucceed()
    {
        // Arrange & Act
        var purchase = Purchase.Create(
            _validTenantId,
            ValidName,
            ValidType,
            ValidAmount,
            ValidQuantity,
            ValidUnit,
            ValidPurchaseDate,
            null,
            ValidPurchaseDate);

        // Assert
        purchase.Should().NotBeNull();
        purchase.ConsumedDate.Should().Be(ValidPurchaseDate);
    }

    [Fact]
    public void Create_WithConsumedDateLocalTime_ShouldConvertToUtc()
    {
        // Arrange
        var localDate = new DateTime(2024, 1, 20, 0, 0, 0, DateTimeKind.Local);
        var purchaseDate = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var purchase = Purchase.Create(
            _validTenantId,
            ValidName,
            ValidType,
            ValidAmount,
            ValidQuantity,
            ValidUnit,
            purchaseDate,
            null,
            localDate);

        // Assert
        purchase.ConsumedDate!.Value.Kind.Should().Be(DateTimeKind.Utc);
    }

    #endregion

    #region Validation Tests - Notes

    [Fact]
    public void Create_WithNotesExceeding500Characters_ShouldThrowArgumentException()
    {
        // Arrange
        var longNotes = new string('A', 501);

        // Act
        var act = () => Purchase.Create(
            _validTenantId,
            ValidName,
            ValidType,
            ValidAmount,
            ValidQuantity,
            ValidUnit,
            ValidPurchaseDate,
            null,
            null,
            longNotes);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Notes cannot exceed 500 characters.*")
            .And.ParamName.Should().Be("notes");
    }

    [Fact]
    public void Create_WithNotesExactly500Characters_ShouldSucceed()
    {
        // Arrange
        var notes = new string('A', 500);

        // Act
        var purchase = Purchase.Create(
            _validTenantId,
            ValidName,
            ValidType,
            ValidAmount,
            ValidQuantity,
            ValidUnit,
            ValidPurchaseDate,
            null,
            null,
            notes);

        // Assert
        purchase.Should().NotBeNull();
        purchase.Notes.Should().Be(notes);
        purchase.Notes!.Length.Should().Be(500);
    }

    [Fact]
    public void Create_WithEmptyStringNotes_ShouldSucceed()
    {
        // Arrange & Act
        var purchase = Purchase.Create(
            _validTenantId,
            ValidName,
            ValidType,
            ValidAmount,
            ValidQuantity,
            ValidUnit,
            ValidPurchaseDate,
            null,
            null,
            string.Empty);

        // Assert
        purchase.Should().NotBeNull();
        purchase.Notes.Should().Be(string.Empty);
    }

    #endregion

    #region Update Tests

    [Fact]
    public void Update_WithValidData_ShouldUpdateAllFields()
    {
        // Arrange
        var purchase = Purchase.Create(
            _validTenantId,
            ValidName,
            ValidType,
            ValidAmount,
            ValidQuantity,
            ValidUnit,
            ValidPurchaseDate);
        var originalCreatedAt = purchase.CreatedAt;
        var originalUpdatedAt = purchase.UpdatedAt;
        Thread.Sleep(10);

        var newName = "Updated Feed";
        var newType = PurchaseType.Vitamins;
        var newAmount = 30.00m;
        var newQuantity = 15.0m;
        var newUnit = QuantityUnit.L;
        var newPurchaseDate = ValidPurchaseDate.AddDays(1);
        var newConsumedDate = ValidPurchaseDate.AddDays(3);
        var newNotes = "Updated notes";

        // Act
        purchase.Update(newName, newType, newAmount, newQuantity, newUnit, newPurchaseDate, _validCoopId, newConsumedDate, newNotes);

        // Assert
        purchase.Name.Should().Be(newName);
        purchase.Type.Should().Be(newType);
        purchase.Amount.Should().Be(newAmount);
        purchase.Quantity.Should().Be(newQuantity);
        purchase.Unit.Should().Be(newUnit);
        purchase.PurchaseDate.Should().Be(newPurchaseDate);
        purchase.CoopId.Should().Be(_validCoopId);
        purchase.ConsumedDate.Should().Be(newConsumedDate);
        purchase.Notes.Should().Be(newNotes);
        purchase.CreatedAt.Should().Be(originalCreatedAt);
        purchase.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void Update_WithoutOptionalFields_ShouldUpdateRequiredFieldsOnly()
    {
        // Arrange
        var purchase = Purchase.Create(
            _validTenantId,
            ValidName,
            ValidType,
            ValidAmount,
            ValidQuantity,
            ValidUnit,
            ValidPurchaseDate,
            _validCoopId,
            ValidPurchaseDate.AddDays(1),
            ValidNotes);

        var newName = "Updated Feed";
        var newType = PurchaseType.Bedding;
        var newAmount = 40.00m;
        var newQuantity = 20.0m;
        var newUnit = QuantityUnit.Package;
        var newPurchaseDate = ValidPurchaseDate.AddDays(2);

        // Act
        purchase.Update(newName, newType, newAmount, newQuantity, newUnit, newPurchaseDate);

        // Assert
        purchase.Name.Should().Be(newName);
        purchase.CoopId.Should().BeNull();
        purchase.ConsumedDate.Should().BeNull();
        purchase.Notes.Should().BeNull();
    }

    [Fact]
    public void Update_WithNegativeAmount_ShouldThrowArgumentException()
    {
        // Arrange
        var purchase = Purchase.Create(
            _validTenantId,
            ValidName,
            ValidType,
            ValidAmount,
            ValidQuantity,
            ValidUnit,
            ValidPurchaseDate);

        // Act
        var act = () => purchase.Update(ValidName, ValidType, -1m, ValidQuantity, ValidUnit, ValidPurchaseDate);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Amount cannot be negative.*")
            .And.ParamName.Should().Be("amount");
    }

    [Fact]
    public void Update_WithZeroQuantity_ShouldThrowArgumentException()
    {
        // Arrange
        var purchase = Purchase.Create(
            _validTenantId,
            ValidName,
            ValidType,
            ValidAmount,
            ValidQuantity,
            ValidUnit,
            ValidPurchaseDate);

        // Act
        var act = () => purchase.Update(ValidName, ValidType, ValidAmount, 0m, ValidUnit, ValidPurchaseDate);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Quantity must be greater than zero.*")
            .And.ParamName.Should().Be("quantity");
    }

    [Fact]
    public void Update_WithConsumedDateBeforePurchaseDate_ShouldThrowArgumentException()
    {
        // Arrange
        var purchase = Purchase.Create(
            _validTenantId,
            ValidName,
            ValidType,
            ValidAmount,
            ValidQuantity,
            ValidUnit,
            ValidPurchaseDate);

        var newPurchaseDate = ValidPurchaseDate.AddDays(5);
        var consumedDate = ValidPurchaseDate.AddDays(2);

        // Act
        var act = () => purchase.Update(ValidName, ValidType, ValidAmount, ValidQuantity, ValidUnit, newPurchaseDate, null, consumedDate);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Consumed date cannot be before purchase date.*")
            .And.ParamName.Should().Be("consumedDate");
    }

    [Fact]
    public void Update_WithEmptyName_ShouldThrowArgumentException()
    {
        // Arrange
        var purchase = Purchase.Create(
            _validTenantId,
            ValidName,
            ValidType,
            ValidAmount,
            ValidQuantity,
            ValidUnit,
            ValidPurchaseDate);

        // Act
        var act = () => purchase.Update(string.Empty, ValidType, ValidAmount, ValidQuantity, ValidUnit, ValidPurchaseDate);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Purchase name cannot be empty.*")
            .And.ParamName.Should().Be("name");
    }

    [Fact]
    public void Update_WithNameExceeding100Characters_ShouldThrowArgumentException()
    {
        // Arrange
        var purchase = Purchase.Create(
            _validTenantId,
            ValidName,
            ValidType,
            ValidAmount,
            ValidQuantity,
            ValidUnit,
            ValidPurchaseDate);

        var longName = new string('B', 101);

        // Act
        var act = () => purchase.Update(longName, ValidType, ValidAmount, ValidQuantity, ValidUnit, ValidPurchaseDate);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Purchase name cannot exceed 100 characters.*")
            .And.ParamName.Should().Be("name");
    }

    [Fact]
    public void Update_WithNotesExceeding500Characters_ShouldThrowArgumentException()
    {
        // Arrange
        var purchase = Purchase.Create(
            _validTenantId,
            ValidName,
            ValidType,
            ValidAmount,
            ValidQuantity,
            ValidUnit,
            ValidPurchaseDate);

        var longNotes = new string('B', 501);

        // Act
        var act = () => purchase.Update(ValidName, ValidType, ValidAmount, ValidQuantity, ValidUnit, ValidPurchaseDate, null, null, longNotes);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Notes cannot exceed 500 characters.*")
            .And.ParamName.Should().Be("notes");
    }

    [Fact]
    public void Update_ShouldNotModifyTenantId()
    {
        // Arrange
        var purchase = Purchase.Create(
            _validTenantId,
            ValidName,
            ValidType,
            ValidAmount,
            ValidQuantity,
            ValidUnit,
            ValidPurchaseDate);

        // Act
        purchase.Update("Updated", ValidType, ValidAmount, ValidQuantity, ValidUnit, ValidPurchaseDate);

        // Assert
        purchase.TenantId.Should().Be(_validTenantId);
    }

    #endregion
}
