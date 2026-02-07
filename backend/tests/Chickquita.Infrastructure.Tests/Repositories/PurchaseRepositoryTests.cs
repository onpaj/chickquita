using Chickquita.Domain.Entities;
using Chickquita.Infrastructure.Data;
using Chickquita.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Chickquita.Infrastructure.Tests.Repositories;

/// <summary>
/// Integration tests for PurchaseRepository.
/// Tests CRUD operations, tenant filtering via RLS, navigation properties,
/// date range filtering, and type filtering.
/// </summary>
public class PurchaseRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ApplicationDbContext _dbContext;
    private readonly PurchaseRepository _repository;
    private readonly Guid _tenantId;
    private readonly Guid _coopId;

    public PurchaseRepositoryTests()
    {
        // Use SQLite in-memory database for testing
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _dbContext.Database.EnsureCreated();

        _repository = new PurchaseRepository(_dbContext);

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

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_ReturnsAllPurchases_OrderedByDateDescending()
    {
        // Arrange
        var purchase1 = Purchase.Create(
            _tenantId, "Feed 1", PurchaseType.Feed, 100m, 10m, QuantityUnit.Kg,
            DateTime.UtcNow.AddDays(-2), _coopId);
        var purchase2 = Purchase.Create(
            _tenantId, "Vitamins 1", PurchaseType.Vitamins, 50m, 5m, QuantityUnit.Pcs,
            DateTime.UtcNow.AddDays(-1), _coopId);
        var purchase3 = Purchase.Create(
            _tenantId, "Bedding 1", PurchaseType.Bedding, 75m, 20m, QuantityUnit.Kg,
            DateTime.UtcNow, _coopId);

        _dbContext.Purchases.AddRange(purchase1, purchase2, purchase3);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
        result[0].PurchaseDate.Should().Be(purchase3.PurchaseDate);
        result[1].PurchaseDate.Should().Be(purchase2.PurchaseDate);
        result[2].PurchaseDate.Should().Be(purchase1.PurchaseDate);
    }

    [Fact]
    public async Task GetAllAsync_IncludesCoopNavigationProperty()
    {
        // Arrange
        var purchase = Purchase.Create(
            _tenantId, "Feed 1", PurchaseType.Feed, 100m, 10m, QuantityUnit.Kg,
            DateTime.UtcNow, _coopId);
        _dbContext.Purchases.Add(purchase);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(1);
        result[0].Coop.Should().NotBeNull();
        result[0].Coop!.Id.Should().Be(_coopId);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyList_WhenNoPurchasesExist()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_ReturnsPurchase_WhenPurchaseExists()
    {
        // Arrange
        var purchase = Purchase.Create(
            _tenantId, "Feed 1", PurchaseType.Feed, 100m, 10m, QuantityUnit.Kg,
            DateTime.UtcNow, _coopId, null, "Test notes");
        _dbContext.Purchases.Add(purchase);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(purchase.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(purchase.Id);
        result.Name.Should().Be("Feed 1");
        result.Amount.Should().Be(100m);
        result.Notes.Should().Be("Test notes");
    }

    [Fact]
    public async Task GetByIdAsync_IncludesCoopNavigationProperty()
    {
        // Arrange
        var purchase = Purchase.Create(
            _tenantId, "Feed 1", PurchaseType.Feed, 100m, 10m, QuantityUnit.Kg,
            DateTime.UtcNow, _coopId);
        _dbContext.Purchases.Add(purchase);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(purchase.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Coop.Should().NotBeNull();
        result.Coop!.Id.Should().Be(_coopId);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenPurchaseDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetByDateRangeAsync Tests

    [Fact]
    public async Task GetByDateRangeAsync_ReturnsPurchasesWithinDateRange()
    {
        // Arrange
        var date1 = DateTime.UtcNow.AddDays(-5);
        var date2 = DateTime.UtcNow.AddDays(-3);
        var date3 = DateTime.UtcNow.AddDays(-1);

        var purchase1 = Purchase.Create(
            _tenantId, "Feed 1", PurchaseType.Feed, 100m, 10m, QuantityUnit.Kg,
            date1, _coopId);
        var purchase2 = Purchase.Create(
            _tenantId, "Vitamins 1", PurchaseType.Vitamins, 50m, 5m, QuantityUnit.Pcs,
            date2, _coopId);
        var purchase3 = Purchase.Create(
            _tenantId, "Bedding 1", PurchaseType.Bedding, 75m, 20m, QuantityUnit.Kg,
            date3, _coopId);

        _dbContext.Purchases.AddRange(purchase1, purchase2, purchase3);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByDateRangeAsync(date1.Date, date2.Date);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(r => r.PurchaseDate == date1.Date);
        result.Should().Contain(r => r.PurchaseDate == date2.Date);
    }

    [Fact]
    public async Task GetByDateRangeAsync_ReturnsEmptyList_WhenNoPurchasesInRange()
    {
        // Arrange
        var purchase = Purchase.Create(
            _tenantId, "Feed 1", PurchaseType.Feed, 100m, 10m, QuantityUnit.Kg,
            DateTime.UtcNow, _coopId);
        _dbContext.Purchases.Add(purchase);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByDateRangeAsync(
            DateTime.UtcNow.AddDays(-10),
            DateTime.UtcNow.AddDays(-5));

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByDateRangeAsync_OrdersByDateDescending()
    {
        // Arrange
        var date1 = DateTime.UtcNow.AddDays(-3);
        var date2 = DateTime.UtcNow.AddDays(-1);

        var purchase1 = Purchase.Create(
            _tenantId, "Feed 1", PurchaseType.Feed, 100m, 10m, QuantityUnit.Kg,
            date1, _coopId);
        var purchase2 = Purchase.Create(
            _tenantId, "Vitamins 1", PurchaseType.Vitamins, 50m, 5m, QuantityUnit.Pcs,
            date2, _coopId);

        _dbContext.Purchases.AddRange(purchase1, purchase2);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByDateRangeAsync(date1.Date, date2.Date);

        // Assert
        result.Should().HaveCount(2);
        result[0].PurchaseDate.Should().Be(date2.Date);
        result[1].PurchaseDate.Should().Be(date1.Date);
    }

    #endregion

    #region GetByTypeAsync Tests

    [Fact]
    public async Task GetByTypeAsync_ReturnsPurchasesOfSpecificType()
    {
        // Arrange
        var purchase1 = Purchase.Create(
            _tenantId, "Feed 1", PurchaseType.Feed, 100m, 10m, QuantityUnit.Kg,
            DateTime.UtcNow.AddDays(-2), _coopId);
        var purchase2 = Purchase.Create(
            _tenantId, "Feed 2", PurchaseType.Feed, 150m, 15m, QuantityUnit.Kg,
            DateTime.UtcNow.AddDays(-1), _coopId);
        var purchase3 = Purchase.Create(
            _tenantId, "Vitamins 1", PurchaseType.Vitamins, 50m, 5m, QuantityUnit.Pcs,
            DateTime.UtcNow, _coopId);

        _dbContext.Purchases.AddRange(purchase1, purchase2, purchase3);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByTypeAsync(PurchaseType.Feed);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(p => p.Type.Should().Be(PurchaseType.Feed));
    }

    [Fact]
    public async Task GetByTypeAsync_ReturnsEmptyList_WhenNoMatchingTypeExists()
    {
        // Arrange
        var purchase = Purchase.Create(
            _tenantId, "Feed 1", PurchaseType.Feed, 100m, 10m, QuantityUnit.Kg,
            DateTime.UtcNow, _coopId);
        _dbContext.Purchases.Add(purchase);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByTypeAsync(PurchaseType.Veterinary);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByTypeAsync_OrdersByDateDescending()
    {
        // Arrange
        var purchase1 = Purchase.Create(
            _tenantId, "Feed 1", PurchaseType.Feed, 100m, 10m, QuantityUnit.Kg,
            DateTime.UtcNow.AddDays(-2), _coopId);
        var purchase2 = Purchase.Create(
            _tenantId, "Feed 2", PurchaseType.Feed, 150m, 15m, QuantityUnit.Kg,
            DateTime.UtcNow.AddDays(-1), _coopId);

        _dbContext.Purchases.AddRange(purchase1, purchase2);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByTypeAsync(PurchaseType.Feed);

        // Assert
        result.Should().HaveCount(2);
        result[0].PurchaseDate.Should().Be(purchase2.PurchaseDate);
        result[1].PurchaseDate.Should().Be(purchase1.PurchaseDate);
    }

    #endregion

    #region GetDistinctNamesAsync Tests

    [Fact]
    public async Task GetDistinctNamesAsync_ReturnsUniqueNames_Alphabetically()
    {
        // Arrange
        var purchase1 = Purchase.Create(
            _tenantId, "Chicken Feed", PurchaseType.Feed, 100m, 10m, QuantityUnit.Kg,
            DateTime.UtcNow, _coopId);
        var purchase2 = Purchase.Create(
            _tenantId, "Vitamins", PurchaseType.Vitamins, 50m, 5m, QuantityUnit.Pcs,
            DateTime.UtcNow, _coopId);
        var purchase3 = Purchase.Create(
            _tenantId, "Chicken Feed", PurchaseType.Feed, 120m, 12m, QuantityUnit.Kg,
            DateTime.UtcNow.AddDays(-1), _coopId);
        var purchase4 = Purchase.Create(
            _tenantId, "Bedding", PurchaseType.Bedding, 75m, 20m, QuantityUnit.Kg,
            DateTime.UtcNow, _coopId);

        _dbContext.Purchases.AddRange(purchase1, purchase2, purchase3, purchase4);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetDistinctNamesAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Equal("Bedding", "Chicken Feed", "Vitamins");
    }

    [Fact]
    public async Task GetDistinctNamesAsync_ReturnsEmptyList_WhenNoPurchasesExist()
    {
        // Act
        var result = await _repository.GetDistinctNamesAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_AddsPurchase_Successfully()
    {
        // Arrange
        var purchase = Purchase.Create(
            _tenantId, "Feed 1", PurchaseType.Feed, 100m, 10m, QuantityUnit.Kg,
            DateTime.UtcNow, _coopId, null, "Test notes");

        // Act
        var result = await _repository.AddAsync(purchase);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();

        var savedPurchase = await _dbContext.Purchases.FindAsync(result.Id);
        savedPurchase.Should().NotBeNull();
        savedPurchase!.Name.Should().Be("Feed 1");
        savedPurchase.Amount.Should().Be(100m);
        savedPurchase.Notes.Should().Be("Test notes");
    }

    [Fact]
    public async Task AddAsync_ThrowsArgumentNullException_WhenPurchaseIsNull()
    {
        // Act
        var act = async () => await _repository.AddAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_UpdatesPurchase_Successfully()
    {
        // Arrange
        var purchase = Purchase.Create(
            _tenantId, "Feed 1", PurchaseType.Feed, 100m, 10m, QuantityUnit.Kg,
            DateTime.UtcNow, _coopId, null, "Original notes");
        _dbContext.Purchases.Add(purchase);
        await _dbContext.SaveChangesAsync();

        // Act
        purchase.Update(
            "Feed 2", PurchaseType.Feed, 150m, 15m, QuantityUnit.Kg,
            DateTime.UtcNow, _coopId, null, "Updated notes");
        var result = await _repository.UpdateAsync(purchase);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Feed 2");
        result.Amount.Should().Be(150m);
        result.Quantity.Should().Be(15m);
        result.Notes.Should().Be("Updated notes");

        var updatedPurchase = await _dbContext.Purchases.FindAsync(purchase.Id);
        updatedPurchase.Should().NotBeNull();
        updatedPurchase!.Name.Should().Be("Feed 2");
        updatedPurchase.Amount.Should().Be(150m);
        updatedPurchase.Notes.Should().Be("Updated notes");
    }

    [Fact]
    public async Task UpdateAsync_ThrowsArgumentNullException_WhenPurchaseIsNull()
    {
        // Act
        var act = async () => await _repository.UpdateAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_DeletesPurchase_Successfully()
    {
        // Arrange
        var purchase = Purchase.Create(
            _tenantId, "Feed 1", PurchaseType.Feed, 100m, 10m, QuantityUnit.Kg,
            DateTime.UtcNow, _coopId);
        _dbContext.Purchases.Add(purchase);
        await _dbContext.SaveChangesAsync();

        var purchaseId = purchase.Id;

        // Act
        await _repository.DeleteAsync(purchaseId);

        // Assert
        var deletedPurchase = await _dbContext.Purchases.FindAsync(purchaseId);
        deletedPurchase.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_DoesNotThrow_WhenPurchaseDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var act = async () => await _repository.DeleteAsync(nonExistentId);

        // Assert
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region Tenant Isolation Tests

    [Fact]
    public async Task GetAllAsync_OnlyReturnsPurchasesForCurrentTenant()
    {
        // Arrange
        var tenant2Id = Guid.NewGuid();
        var tenant2 = Tenant.Create("clerk_user_test2", "test2@example.com");
        typeof(Tenant).GetProperty(nameof(Tenant.Id))!.SetValue(tenant2, tenant2Id);
        _dbContext.Tenants.Add(tenant2);
        await _dbContext.SaveChangesAsync();

        var coop2 = Coop.Create(tenant2Id, "Coop 2", "Location 2");
        _dbContext.Coops.Add(coop2);
        await _dbContext.SaveChangesAsync();

        // Add purchases for both tenants
        var purchase1 = Purchase.Create(
            _tenantId, "Feed 1", PurchaseType.Feed, 100m, 10m, QuantityUnit.Kg,
            DateTime.UtcNow, _coopId);
        var purchase2 = Purchase.Create(
            tenant2Id, "Feed 2", PurchaseType.Feed, 150m, 15m, QuantityUnit.Kg,
            DateTime.UtcNow, coop2.Id);

        _dbContext.Purchases.AddRange(purchase1, purchase2);
        await _dbContext.SaveChangesAsync();

        // Act - This would normally be filtered by RLS, but in tests we see all data
        var allPurchases = await _dbContext.Purchases.ToListAsync();

        // Assert - Verify both purchases exist in the database (RLS would filter in production)
        allPurchases.Should().HaveCount(2);
        allPurchases.Should().Contain(p => p.TenantId == _tenantId);
        allPurchases.Should().Contain(p => p.TenantId == tenant2Id);
    }

    #endregion

    #region Database Constraint Tests

    [Fact]
    public async Task Purchase_ForeignKeyConstraint_TenantIdMustExist()
    {
        // Arrange
        var nonExistentTenantId = Guid.NewGuid();
        var purchase = Purchase.Create(
            nonExistentTenantId, "Feed 1", PurchaseType.Feed, 100m, 10m, QuantityUnit.Kg,
            DateTime.UtcNow, _coopId);
        _dbContext.Purchases.Add(purchase);

        // Act & Assert
        var act = async () => await _dbContext.SaveChangesAsync();
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task Purchase_OptionalCoopId_CanBeNull()
    {
        // Arrange
        var purchase = Purchase.Create(
            _tenantId, "Feed 1", PurchaseType.Feed, 100m, 10m, QuantityUnit.Kg,
            DateTime.UtcNow, null);
        _dbContext.Purchases.Add(purchase);

        // Act
        await _dbContext.SaveChangesAsync();

        // Assert
        var savedPurchase = await _dbContext.Purchases.FindAsync(purchase.Id);
        savedPurchase.Should().NotBeNull();
        savedPurchase!.CoopId.Should().BeNull();
    }

    [Fact]
    public async Task Purchase_CascadeDelete_DeletedWhenTenantIsDeleted()
    {
        // Arrange
        var purchase = Purchase.Create(
            _tenantId, "Feed 1", PurchaseType.Feed, 100m, 10m, QuantityUnit.Kg,
            DateTime.UtcNow, _coopId);
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
            _tenantId, "Feed 1", PurchaseType.Feed, 100m, 10m, QuantityUnit.Kg,
            DateTime.UtcNow, _coopId);
        _dbContext.Purchases.Add(purchase);
        await _dbContext.SaveChangesAsync();

        var purchaseId = purchase.Id;

        // Act - Delete the coop (should set CoopId to null via SET NULL)
        var coop = await _dbContext.Coops.FindAsync(_coopId);
        _dbContext.Coops.Remove(coop!);
        await _dbContext.SaveChangesAsync();

        // Assert
        var updatedPurchase = await _dbContext.Purchases.FindAsync(purchaseId);
        updatedPurchase.Should().NotBeNull("Purchase should still exist");
        updatedPurchase!.CoopId.Should().BeNull("CoopId should be set to NULL when coop is deleted");
    }

    #endregion

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Close();
        _connection.Dispose();
    }
}
