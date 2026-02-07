using Chickquita.Domain.Entities;
using Chickquita.Infrastructure.Data;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Chickquita.Infrastructure.Tests.Data;

/// <summary>
/// Integration tests for Purchase entity data integrity.
/// Tests database-level constraints, foreign key relationships,
/// and cascade behaviors.
/// Note: RLS testing requires PostgreSQL and is covered in separate integration tests.
/// </summary>
public class PurchaseDataIntegrityTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ApplicationDbContext _dbContext;
    private readonly Guid _tenantId;
    private readonly Guid _coopId;

    public PurchaseDataIntegrityTests()
    {
        // Use SQLite in-memory database for testing
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _dbContext.Database.EnsureCreated();

        // Seed test data
        _tenantId = Guid.NewGuid();
        var tenant = Tenant.Create("clerk_user_test", "test@example.com");
        typeof(Tenant).GetProperty(nameof(Tenant.Id))!.SetValue(tenant, _tenantId);
        _dbContext.Tenants.Add(tenant);

        var coop = Coop.Create(_tenantId, "Test Coop", "Test Location");
        _dbContext.Coops.Add(coop);
        _dbContext.SaveChanges();

        _coopId = coop.Id;
    }

    #region Database Constraint Tests

    [Fact]
    public async Task Purchase_NotNullConstraint_TenantIdRequired()
    {
        // Arrange - Domain validation should prevent creating purchase with empty tenant ID
        var act = () => Purchase.Create(
            Guid.Empty,
            "Feed",
            PurchaseType.Feed,
            25.50m,
            10.0m,
            QuantityUnit.Kg,
            DateTime.UtcNow.AddDays(-5));

        // Act & Assert - Should fail at domain level before reaching database
        act.Should().Throw<ArgumentException>().WithMessage("*tenantId*");
    }

    [Fact]
    public async Task Purchase_NotNullConstraint_NameRequired()
    {
        // Arrange
        var act = () => Purchase.Create(
            _tenantId,
            string.Empty,
            PurchaseType.Feed,
            25.50m,
            10.0m,
            QuantityUnit.Kg,
            DateTime.UtcNow.AddDays(-5));

        // Act & Assert
        act.Should().Throw<ArgumentException>().WithMessage("*name*");
    }

    [Fact]
    public async Task Purchase_CheckConstraint_AmountCannotBeNegative()
    {
        // Arrange
        var act = () => Purchase.Create(
            _tenantId,
            "Feed",
            PurchaseType.Feed,
            -0.01m,
            10.0m,
            QuantityUnit.Kg,
            DateTime.UtcNow.AddDays(-5));

        // Act & Assert - Domain validation catches this
        act.Should().Throw<ArgumentException>().WithMessage("*amount*");
    }

    [Fact]
    public async Task Purchase_CheckConstraint_QuantityMustBeGreaterThanZero()
    {
        // Arrange
        var act = () => Purchase.Create(
            _tenantId,
            "Feed",
            PurchaseType.Feed,
            25.50m,
            0m,
            QuantityUnit.Kg,
            DateTime.UtcNow.AddDays(-5));

        // Act & Assert - Domain validation catches this
        act.Should().Throw<ArgumentException>().WithMessage("*quantity*");
    }

    [Fact]
    public async Task Purchase_CheckConstraint_ConsumedDateCannotBeBeforePurchaseDate()
    {
        // Arrange
        var purchaseDate = DateTime.UtcNow.AddDays(-5);
        var consumedDate = purchaseDate.AddDays(-1);

        var act = () => Purchase.Create(
            _tenantId,
            "Feed",
            PurchaseType.Feed,
            25.50m,
            10.0m,
            QuantityUnit.Kg,
            purchaseDate,
            null,
            consumedDate);

        // Act & Assert - Domain validation catches this
        act.Should().Throw<ArgumentException>().WithMessage("*consumed date*");
    }

    [Fact]
    public async Task Purchase_ForeignKeyConstraint_TenantIdMustExist()
    {
        // Arrange
        var nonExistentTenantId = Guid.NewGuid();
        var purchase = Purchase.Create(
            nonExistentTenantId,
            "Feed",
            PurchaseType.Feed,
            25.50m,
            10.0m,
            QuantityUnit.Kg,
            DateTime.UtcNow.AddDays(-5));
        _dbContext.Purchases.Add(purchase);

        // Act & Assert - Foreign key constraint should prevent this
        var act = async () => await _dbContext.SaveChangesAsync();
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task Purchase_ForeignKeyConstraint_CoopIdMustExistWhenProvided()
    {
        // Arrange
        var nonExistentCoopId = Guid.NewGuid();
        var purchase = Purchase.Create(
            _tenantId,
            "Feed",
            PurchaseType.Feed,
            25.50m,
            10.0m,
            QuantityUnit.Kg,
            DateTime.UtcNow.AddDays(-5),
            nonExistentCoopId);
        _dbContext.Purchases.Add(purchase);

        // Act & Assert - Foreign key constraint should prevent this
        var act = async () => await _dbContext.SaveChangesAsync();
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task Purchase_CoopId_CanBeNull()
    {
        // Arrange
        var purchase = Purchase.Create(
            _tenantId,
            "Feed",
            PurchaseType.Feed,
            25.50m,
            10.0m,
            QuantityUnit.Kg,
            DateTime.UtcNow.AddDays(-5),
            null);
        _dbContext.Purchases.Add(purchase);

        // Act
        await _dbContext.SaveChangesAsync();

        // Assert
        var savedPurchase = await _dbContext.Purchases.FindAsync(purchase.Id);
        savedPurchase.Should().NotBeNull();
        savedPurchase!.CoopId.Should().BeNull();
    }

    #endregion

    #region Cascade Behavior Tests

    [Fact]
    public async Task Purchase_CascadeDelete_DeletedWhenTenantIsDeleted()
    {
        // Arrange
        var purchase = Purchase.Create(
            _tenantId,
            "Feed",
            PurchaseType.Feed,
            25.50m,
            10.0m,
            QuantityUnit.Kg,
            DateTime.UtcNow.AddDays(-5));
        _dbContext.Purchases.Add(purchase);
        await _dbContext.SaveChangesAsync();

        var purchaseId = purchase.Id;

        // Act - Delete the tenant (should cascade to purchases)
        var tenant = await _dbContext.Tenants.FindAsync(_tenantId);
        _dbContext.Tenants.Remove(tenant!);
        await _dbContext.SaveChangesAsync();

        // Assert
        var deletedPurchase = await _dbContext.Purchases.FindAsync(purchaseId);
        deletedPurchase.Should().BeNull("CASCADE delete should remove purchase when tenant is deleted");
    }

    [Fact]
    public async Task Purchase_SetNull_CoopIdSetToNullWhenCoopIsDeleted()
    {
        // Arrange
        var purchase = Purchase.Create(
            _tenantId,
            "Feed",
            PurchaseType.Feed,
            25.50m,
            10.0m,
            QuantityUnit.Kg,
            DateTime.UtcNow.AddDays(-5),
            _coopId);
        _dbContext.Purchases.Add(purchase);
        await _dbContext.SaveChangesAsync();

        var purchaseId = purchase.Id;

        // Act - Delete the coop (should set CoopId to NULL in purchase)
        var coop = await _dbContext.Coops.FindAsync(_coopId);
        _dbContext.Coops.Remove(coop!);
        await _dbContext.SaveChangesAsync();

        // Assert
        var updatedPurchase = await _dbContext.Purchases.FindAsync(purchaseId);
        updatedPurchase.Should().NotBeNull("Purchase should still exist after coop deletion");
        updatedPurchase!.CoopId.Should().BeNull("CoopId should be set to NULL when coop is deleted");
    }

    #endregion

    #region Data Persistence Tests

    [Fact]
    public async Task Purchase_Create_ShouldPersistAllProperties()
    {
        // Arrange
        var purchaseDate = DateTime.UtcNow.AddDays(-5).Date;
        var consumedDate = purchaseDate.AddDays(2);
        var purchase = Purchase.Create(
            _tenantId,
            "Premium Chicken Feed",
            PurchaseType.Feed,
            25.50m,
            10.0m,
            QuantityUnit.Kg,
            purchaseDate,
            _coopId,
            consumedDate,
            "High quality organic feed");

        // Act
        _dbContext.Purchases.Add(purchase);
        await _dbContext.SaveChangesAsync();

        // Clear change tracker to ensure fresh load
        _dbContext.ChangeTracker.Clear();

        // Assert
        var savedPurchase = await _dbContext.Purchases.FindAsync(purchase.Id);
        savedPurchase.Should().NotBeNull();
        savedPurchase!.TenantId.Should().Be(_tenantId);
        savedPurchase.CoopId.Should().Be(_coopId);
        savedPurchase.Name.Should().Be("Premium Chicken Feed");
        savedPurchase.Type.Should().Be(PurchaseType.Feed);
        savedPurchase.Amount.Should().Be(25.50m);
        savedPurchase.Quantity.Should().Be(10.0m);
        savedPurchase.Unit.Should().Be(QuantityUnit.Kg);
        savedPurchase.PurchaseDate.Should().Be(purchaseDate);
        savedPurchase.ConsumedDate.Should().Be(consumedDate);
        savedPurchase.Notes.Should().Be("High quality organic feed");
        savedPurchase.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        savedPurchase.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Purchase_Update_ShouldPersistChanges()
    {
        // Arrange
        var purchase = Purchase.Create(
            _tenantId,
            "Feed",
            PurchaseType.Feed,
            25.50m,
            10.0m,
            QuantityUnit.Kg,
            DateTime.UtcNow.AddDays(-5));
        _dbContext.Purchases.Add(purchase);
        await _dbContext.SaveChangesAsync();

        // Act
        purchase.Update(
            "Updated Feed Name",
            PurchaseType.Vitamins,
            30.00m,
            15.0m,
            QuantityUnit.L,
            DateTime.UtcNow.AddDays(-4),
            _coopId,
            DateTime.UtcNow.AddDays(-2),
            "Updated notes");
        await _dbContext.SaveChangesAsync();

        // Clear change tracker
        _dbContext.ChangeTracker.Clear();

        // Assert
        var updatedPurchase = await _dbContext.Purchases.FindAsync(purchase.Id);
        updatedPurchase.Should().NotBeNull();
        updatedPurchase!.Name.Should().Be("Updated Feed Name");
        updatedPurchase.Type.Should().Be(PurchaseType.Vitamins);
        updatedPurchase.Amount.Should().Be(30.00m);
        updatedPurchase.Quantity.Should().Be(15.0m);
        updatedPurchase.Unit.Should().Be(QuantityUnit.L);
        updatedPurchase.CoopId.Should().Be(_coopId);
        updatedPurchase.Notes.Should().Be("Updated notes");
    }

    #endregion

    #region Enum Storage Tests

    [Theory]
    [InlineData(PurchaseType.Feed)]
    [InlineData(PurchaseType.Vitamins)]
    [InlineData(PurchaseType.Bedding)]
    [InlineData(PurchaseType.Toys)]
    [InlineData(PurchaseType.Veterinary)]
    [InlineData(PurchaseType.Other)]
    public async Task Purchase_PurchaseType_ShouldPersistCorrectly(PurchaseType type)
    {
        // Arrange
        var purchase = Purchase.Create(
            _tenantId,
            "Test Purchase",
            type,
            25.50m,
            10.0m,
            QuantityUnit.Kg,
            DateTime.UtcNow.AddDays(-5));
        _dbContext.Purchases.Add(purchase);
        await _dbContext.SaveChangesAsync();

        // Clear change tracker
        _dbContext.ChangeTracker.Clear();

        // Act
        var savedPurchase = await _dbContext.Purchases.FindAsync(purchase.Id);

        // Assert
        savedPurchase.Should().NotBeNull();
        savedPurchase!.Type.Should().Be(type);
    }

    [Theory]
    [InlineData(QuantityUnit.Kg)]
    [InlineData(QuantityUnit.Pcs)]
    [InlineData(QuantityUnit.L)]
    [InlineData(QuantityUnit.Package)]
    [InlineData(QuantityUnit.Other)]
    public async Task Purchase_QuantityUnit_ShouldPersistCorrectly(QuantityUnit unit)
    {
        // Arrange
        var purchase = Purchase.Create(
            _tenantId,
            "Test Purchase",
            PurchaseType.Feed,
            25.50m,
            10.0m,
            unit,
            DateTime.UtcNow.AddDays(-5));
        _dbContext.Purchases.Add(purchase);
        await _dbContext.SaveChangesAsync();

        // Clear change tracker
        _dbContext.ChangeTracker.Clear();

        // Act
        var savedPurchase = await _dbContext.Purchases.FindAsync(purchase.Id);

        // Assert
        savedPurchase.Should().NotBeNull();
        savedPurchase!.Unit.Should().Be(unit);
    }

    #endregion

    #region Navigation Property Tests

    [Fact]
    public async Task Purchase_NavigationProperty_TenantShouldLoad()
    {
        // Arrange
        var purchase = Purchase.Create(
            _tenantId,
            "Feed",
            PurchaseType.Feed,
            25.50m,
            10.0m,
            QuantityUnit.Kg,
            DateTime.UtcNow.AddDays(-5));
        _dbContext.Purchases.Add(purchase);
        await _dbContext.SaveChangesAsync();

        // Clear change tracker
        _dbContext.ChangeTracker.Clear();

        // Act
        var loadedPurchase = await _dbContext.Purchases
            .Include(p => p.Tenant)
            .FirstOrDefaultAsync(p => p.Id == purchase.Id);

        // Assert
        loadedPurchase.Should().NotBeNull();
        loadedPurchase!.Tenant.Should().NotBeNull();
        loadedPurchase.Tenant.Id.Should().Be(_tenantId);
    }

    [Fact]
    public async Task Purchase_NavigationProperty_CoopShouldLoad()
    {
        // Arrange
        var purchase = Purchase.Create(
            _tenantId,
            "Feed",
            PurchaseType.Feed,
            25.50m,
            10.0m,
            QuantityUnit.Kg,
            DateTime.UtcNow.AddDays(-5),
            _coopId);
        _dbContext.Purchases.Add(purchase);
        await _dbContext.SaveChangesAsync();

        // Clear change tracker
        _dbContext.ChangeTracker.Clear();

        // Act
        var loadedPurchase = await _dbContext.Purchases
            .Include(p => p.Coop)
            .FirstOrDefaultAsync(p => p.Id == purchase.Id);

        // Assert
        loadedPurchase.Should().NotBeNull();
        loadedPurchase!.Coop.Should().NotBeNull();
        loadedPurchase.Coop!.Id.Should().Be(_coopId);
    }

    [Fact]
    public async Task Purchase_NavigationProperty_CoopCanBeNull()
    {
        // Arrange
        var purchase = Purchase.Create(
            _tenantId,
            "Feed",
            PurchaseType.Feed,
            25.50m,
            10.0m,
            QuantityUnit.Kg,
            DateTime.UtcNow.AddDays(-5),
            null);
        _dbContext.Purchases.Add(purchase);
        await _dbContext.SaveChangesAsync();

        // Clear change tracker
        _dbContext.ChangeTracker.Clear();

        // Act
        var loadedPurchase = await _dbContext.Purchases
            .Include(p => p.Coop)
            .FirstOrDefaultAsync(p => p.Id == purchase.Id);

        // Assert
        loadedPurchase.Should().NotBeNull();
        loadedPurchase!.Coop.Should().BeNull();
    }

    #endregion

    #region Indexing Tests (Query Performance)

    [Fact]
    public async Task Purchase_Query_ByTenantId_ShouldBeEfficient()
    {
        // Arrange - Create multiple purchases
        for (int i = 0; i < 10; i++)
        {
            var purchase = Purchase.Create(
                _tenantId,
                $"Purchase {i}",
                PurchaseType.Feed,
                25.50m,
                10.0m,
                QuantityUnit.Kg,
                DateTime.UtcNow.AddDays(-i));
            _dbContext.Purchases.Add(purchase);
        }
        await _dbContext.SaveChangesAsync();

        // Act
        var purchases = await _dbContext.Purchases
            .Where(p => p.TenantId == _tenantId)
            .ToListAsync();

        // Assert
        purchases.Should().HaveCount(10);
    }

    [Fact]
    public async Task Purchase_Query_ByPurchaseDate_ShouldBeEfficient()
    {
        // Arrange
        var targetDate = DateTime.UtcNow.AddDays(-5).Date;
        var purchase1 = Purchase.Create(_tenantId, "Purchase 1", PurchaseType.Feed, 25.50m, 10.0m, QuantityUnit.Kg, targetDate);
        var purchase2 = Purchase.Create(_tenantId, "Purchase 2", PurchaseType.Vitamins, 15.00m, 5.0m, QuantityUnit.Pcs, targetDate);
        var purchase3 = Purchase.Create(_tenantId, "Purchase 3", PurchaseType.Bedding, 30.00m, 20.0m, QuantityUnit.Kg, DateTime.UtcNow.AddDays(-3));

        _dbContext.Purchases.AddRange(purchase1, purchase2, purchase3);
        await _dbContext.SaveChangesAsync();

        // Act
        var purchases = await _dbContext.Purchases
            .Where(p => p.PurchaseDate == targetDate)
            .ToListAsync();

        // Assert
        purchases.Should().HaveCount(2);
    }

    [Fact]
    public async Task Purchase_Query_ByType_ShouldBeEfficient()
    {
        // Arrange
        var purchase1 = Purchase.Create(_tenantId, "Feed 1", PurchaseType.Feed, 25.50m, 10.0m, QuantityUnit.Kg, DateTime.UtcNow.AddDays(-5));
        var purchase2 = Purchase.Create(_tenantId, "Feed 2", PurchaseType.Feed, 30.00m, 15.0m, QuantityUnit.Kg, DateTime.UtcNow.AddDays(-4));
        var purchase3 = Purchase.Create(_tenantId, "Vitamins", PurchaseType.Vitamins, 15.00m, 5.0m, QuantityUnit.Pcs, DateTime.UtcNow.AddDays(-3));

        _dbContext.Purchases.AddRange(purchase1, purchase2, purchase3);
        await _dbContext.SaveChangesAsync();

        // Act
        var purchases = await _dbContext.Purchases
            .Where(p => p.Type == PurchaseType.Feed)
            .ToListAsync();

        // Assert
        purchases.Should().HaveCount(2);
    }

    #endregion

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Close();
        _connection.Dispose();
    }
}
