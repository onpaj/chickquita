using Chickquita.Domain.Entities;
using Chickquita.Infrastructure.Data;
using Chickquita.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Chickquita.Infrastructure.Tests.Repositories;

/// <summary>
/// Integration tests for DailyRecordRepository.
/// Tests CRUD operations, tenant filtering via RLS, navigation properties,
/// and unique constraints.
/// </summary>
public class DailyRecordRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ApplicationDbContext _dbContext;
    private readonly DailyRecordRepository _repository;
    private readonly Guid _tenantId;
    private readonly Guid _coopId;
    private readonly Guid _flockId;

    public DailyRecordRepositoryTests()
    {
        // Use SQLite in-memory database for testing
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _dbContext.Database.EnsureCreated();

        _repository = new DailyRecordRepository(_dbContext);

        // Seed test data
        _tenantId = Guid.NewGuid();
        var tenant = Tenant.Create("clerk_user_test", "test@example.com");
        typeof(Tenant).GetProperty(nameof(Tenant.Id))!.SetValue(tenant, _tenantId);
        _dbContext.Tenants.Add(tenant);

        var coop = Coop.Create(_tenantId, "Test Coop", "Test Location");
        _dbContext.Coops.Add(coop);
        _dbContext.SaveChanges();
        _coopId = coop.Id;

        var flock = Flock.Create(_tenantId, _coopId, "TEST-FLOCK", DateTime.UtcNow.AddMonths(-2), 10, 2, 5, null);
        _dbContext.Flocks.Add(flock);
        _dbContext.SaveChanges();
        _flockId = flock.Id;
    }

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_ReturnsAllDailyRecords_OrderedByDateDescending()
    {
        // Arrange
        var record1 = DailyRecord.Create(_tenantId, _flockId, DateTime.UtcNow.AddDays(-2), 10, "Record 1");
        var record2 = DailyRecord.Create(_tenantId, _flockId, DateTime.UtcNow.AddDays(-1), 15, "Record 2");
        var record3 = DailyRecord.Create(_tenantId, _flockId, DateTime.UtcNow, 12, "Record 3");

        _dbContext.DailyRecords.AddRange(record1, record2, record3);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
        result[0].RecordDate.Should().Be(record3.RecordDate);
        result[1].RecordDate.Should().Be(record2.RecordDate);
        result[2].RecordDate.Should().Be(record1.RecordDate);
    }

    [Fact]
    public async Task GetAllAsync_IncludesFlockNavigationProperty()
    {
        // Arrange
        var record = DailyRecord.Create(_tenantId, _flockId, DateTime.UtcNow, 10, null);
        _dbContext.DailyRecords.Add(record);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(1);
        result[0].Flock.Should().NotBeNull();
        result[0].Flock.Id.Should().Be(_flockId);
    }

    #endregion

    #region GetByFlockIdAsync Tests

    [Fact]
    public async Task GetByFlockIdAsync_ReturnsRecordsForSpecificFlock()
    {
        // Arrange
        var flock2 = Flock.Create(_tenantId, _coopId, "FLOCK-2", DateTime.UtcNow.AddMonths(-1), 5, 1, 2, null);
        _dbContext.Flocks.Add(flock2);
        await _dbContext.SaveChangesAsync();

        var record1 = DailyRecord.Create(_tenantId, _flockId, DateTime.UtcNow.AddDays(-1), 10, null);
        var record2 = DailyRecord.Create(_tenantId, flock2.Id, DateTime.UtcNow.AddDays(-1), 5, null);
        var record3 = DailyRecord.Create(_tenantId, _flockId, DateTime.UtcNow, 12, null);

        _dbContext.DailyRecords.AddRange(record1, record2, record3);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByFlockIdAsync(_flockId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(r => r.FlockId.Should().Be(_flockId));
    }

    [Fact]
    public async Task GetByFlockIdAsync_ReturnsEmptyList_WhenNoRecordsExist()
    {
        // Arrange
        var nonExistentFlockId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByFlockIdAsync(nonExistentFlockId);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetByFlockIdAndDateRangeAsync Tests

    [Fact]
    public async Task GetByFlockIdAndDateRangeAsync_ReturnsRecordsWithinDateRange()
    {
        // Arrange
        var date1 = DateTime.UtcNow.AddDays(-5);
        var date2 = DateTime.UtcNow.AddDays(-3);
        var date3 = DateTime.UtcNow.AddDays(-1);

        var record1 = DailyRecord.Create(_tenantId, _flockId, date1, 10, null);
        var record2 = DailyRecord.Create(_tenantId, _flockId, date2, 15, null);
        var record3 = DailyRecord.Create(_tenantId, _flockId, date3, 12, null);

        _dbContext.DailyRecords.AddRange(record1, record2, record3);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByFlockIdAndDateRangeAsync(_flockId, date1.Date, date2.Date);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(r => r.RecordDate == date1.Date);
        result.Should().Contain(r => r.RecordDate == date2.Date);
    }

    [Fact]
    public async Task GetByFlockIdAndDateRangeAsync_ReturnsEmptyList_WhenNoRecordsInRange()
    {
        // Arrange
        var record = DailyRecord.Create(_tenantId, _flockId, DateTime.UtcNow, 10, null);
        _dbContext.DailyRecords.Add(record);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByFlockIdAndDateRangeAsync(
            _flockId,
            DateTime.UtcNow.AddDays(-10),
            DateTime.UtcNow.AddDays(-5));

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_ReturnsRecord_WhenRecordExists()
    {
        // Arrange
        var record = DailyRecord.Create(_tenantId, _flockId, DateTime.UtcNow, 10, "Test record");
        _dbContext.DailyRecords.Add(record);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(record.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(record.Id);
        result.EggCount.Should().Be(10);
        result.Notes.Should().Be("Test record");
    }

    [Fact]
    public async Task GetByIdAsync_IncludesFlockNavigationProperty()
    {
        // Arrange
        var record = DailyRecord.Create(_tenantId, _flockId, DateTime.UtcNow, 10, null);
        _dbContext.DailyRecords.Add(record);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(record.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Flock.Should().NotBeNull();
        result.Flock.Id.Should().Be(_flockId);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenRecordDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetByIdWithoutNavigationAsync Tests

    [Fact]
    public async Task GetByIdWithoutNavigationAsync_ReturnsRecord_WithoutNavigationProperties()
    {
        // Arrange
        var record = DailyRecord.Create(_tenantId, _flockId, DateTime.UtcNow, 10, null);
        _dbContext.DailyRecords.Add(record);
        await _dbContext.SaveChangesAsync();

        // Clear change tracker to ensure no navigation properties are loaded
        _dbContext.ChangeTracker.Clear();

        // Act
        var result = await _repository.GetByIdWithoutNavigationAsync(record.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(record.Id);
    }

    #endregion

    #region GetByFlockIdAndDateAsync Tests

    [Fact]
    public async Task GetByFlockIdAndDateAsync_ReturnsRecord_WhenRecordExists()
    {
        // Arrange
        var recordDate = DateTime.UtcNow.Date;
        var record = DailyRecord.Create(_tenantId, _flockId, recordDate, 10, null);
        _dbContext.DailyRecords.Add(record);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByFlockIdAndDateAsync(_flockId, recordDate);

        // Assert
        result.Should().NotBeNull();
        result!.FlockId.Should().Be(_flockId);
        result.RecordDate.Should().Be(recordDate);
    }

    [Fact]
    public async Task GetByFlockIdAndDateAsync_ReturnsNull_WhenRecordDoesNotExist()
    {
        // Act
        var result = await _repository.GetByFlockIdAndDateAsync(_flockId, DateTime.UtcNow);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_AddsDailyRecord_Successfully()
    {
        // Arrange
        var record = DailyRecord.Create(_tenantId, _flockId, DateTime.UtcNow, 10, "Test record");

        // Act
        var result = await _repository.AddAsync(record);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();

        var savedRecord = await _dbContext.DailyRecords.FindAsync(result.Id);
        savedRecord.Should().NotBeNull();
        savedRecord!.EggCount.Should().Be(10);
        savedRecord.Notes.Should().Be("Test record");
    }

    [Fact]
    public async Task AddAsync_ThrowsArgumentNullException_WhenRecordIsNull()
    {
        // Act
        var act = async () => await _repository.AddAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_UpdatesDailyRecord_Successfully()
    {
        // Arrange
        var record = DailyRecord.Create(_tenantId, _flockId, DateTime.UtcNow, 10, "Original notes");
        _dbContext.DailyRecords.Add(record);
        await _dbContext.SaveChangesAsync();

        // Act
        record.Update(15, "Updated notes");
        var result = await _repository.UpdateAsync(record);

        // Assert
        result.Should().NotBeNull();
        result.EggCount.Should().Be(15);
        result.Notes.Should().Be("Updated notes");

        var updatedRecord = await _dbContext.DailyRecords.FindAsync(record.Id);
        updatedRecord.Should().NotBeNull();
        updatedRecord!.EggCount.Should().Be(15);
        updatedRecord.Notes.Should().Be("Updated notes");
    }

    [Fact]
    public async Task UpdateAsync_ThrowsArgumentNullException_WhenRecordIsNull()
    {
        // Act
        var act = async () => await _repository.UpdateAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_DeletesDailyRecord_Successfully()
    {
        // Arrange
        var record = DailyRecord.Create(_tenantId, _flockId, DateTime.UtcNow, 10, null);
        _dbContext.DailyRecords.Add(record);
        await _dbContext.SaveChangesAsync();

        var recordId = record.Id;

        // Act
        await _repository.DeleteAsync(recordId);

        // Assert
        var deletedRecord = await _dbContext.DailyRecords.FindAsync(recordId);
        deletedRecord.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_DoesNotThrow_WhenRecordDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var act = async () => await _repository.DeleteAsync(nonExistentId);

        // Assert
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region ExistsForFlockAndDateAsync Tests

    [Fact]
    public async Task ExistsForFlockAndDateAsync_ReturnsTrue_WhenRecordExists()
    {
        // Arrange
        var recordDate = DateTime.UtcNow.Date;
        var record = DailyRecord.Create(_tenantId, _flockId, recordDate, 10, null);
        _dbContext.DailyRecords.Add(record);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsForFlockAndDateAsync(_flockId, recordDate);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsForFlockAndDateAsync_ReturnsFalse_WhenRecordDoesNotExist()
    {
        // Act
        var result = await _repository.ExistsForFlockAndDateAsync(_flockId, DateTime.UtcNow);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsForFlockAndDateAsync_WithExcludeRecordId_ExcludesSpecifiedRecord()
    {
        // Arrange
        var recordDate = DateTime.UtcNow.Date;
        var record = DailyRecord.Create(_tenantId, _flockId, recordDate, 10, null);
        _dbContext.DailyRecords.Add(record);
        await _dbContext.SaveChangesAsync();

        // Act - Exclude the existing record
        var result = await _repository.ExistsForFlockAndDateAsync(_flockId, recordDate, record.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsForFlockAndDateAsync_WithExcludeRecordId_ReturnsTrue_WhenOtherRecordExists()
    {
        // Arrange - This scenario shouldn't happen due to unique constraint, but test the logic
        var recordDate = DateTime.UtcNow.Date;
        var record = DailyRecord.Create(_tenantId, _flockId, recordDate, 10, null);
        _dbContext.DailyRecords.Add(record);
        await _dbContext.SaveChangesAsync();

        var otherRecordId = Guid.NewGuid();

        // Act - Exclude a different record
        var result = await _repository.ExistsForFlockAndDateAsync(_flockId, recordDate, otherRecordId);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region GetCountByFlockIdAsync Tests

    [Fact]
    public async Task GetCountByFlockIdAsync_ReturnsCorrectCount()
    {
        // Arrange
        var record1 = DailyRecord.Create(_tenantId, _flockId, DateTime.UtcNow.AddDays(-2), 10, null);
        var record2 = DailyRecord.Create(_tenantId, _flockId, DateTime.UtcNow.AddDays(-1), 15, null);
        var record3 = DailyRecord.Create(_tenantId, _flockId, DateTime.UtcNow, 12, null);

        _dbContext.DailyRecords.AddRange(record1, record2, record3);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetCountByFlockIdAsync(_flockId);

        // Assert
        result.Should().Be(3);
    }

    [Fact]
    public async Task GetCountByFlockIdAsync_ReturnsZero_WhenNoRecordsExist()
    {
        // Act
        var result = await _repository.GetCountByFlockIdAsync(_flockId);

        // Assert
        result.Should().Be(0);
    }

    #endregion

    #region GetTotalEggCountByFlockIdAsync Tests

    [Fact]
    public async Task GetTotalEggCountByFlockIdAsync_ReturnsSumOfEggCounts()
    {
        // Arrange
        var record1 = DailyRecord.Create(_tenantId, _flockId, DateTime.UtcNow.AddDays(-2), 10, null);
        var record2 = DailyRecord.Create(_tenantId, _flockId, DateTime.UtcNow.AddDays(-1), 15, null);
        var record3 = DailyRecord.Create(_tenantId, _flockId, DateTime.UtcNow, 12, null);

        _dbContext.DailyRecords.AddRange(record1, record2, record3);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetTotalEggCountByFlockIdAsync(_flockId);

        // Assert
        result.Should().Be(37); // 10 + 15 + 12
    }

    [Fact]
    public async Task GetTotalEggCountByFlockIdAsync_ReturnsZero_WhenNoRecordsExist()
    {
        // Act
        var result = await _repository.GetTotalEggCountByFlockIdAsync(_flockId);

        // Assert
        result.Should().Be(0);
    }

    #endregion

    #region Database Constraint Tests

    [Fact]
    public async Task DailyRecord_UniqueConstraint_OneRecordPerFlockPerDate()
    {
        // Arrange
        var recordDate = DateTime.UtcNow.Date;
        var record1 = DailyRecord.Create(_tenantId, _flockId, recordDate, 10, null);
        _dbContext.DailyRecords.Add(record1);
        await _dbContext.SaveChangesAsync();

        var record2 = DailyRecord.Create(_tenantId, _flockId, recordDate, 15, null);
        _dbContext.DailyRecords.Add(record2);

        // Act & Assert
        var act = async () => await _dbContext.SaveChangesAsync();
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task DailyRecord_ForeignKeyConstraint_FlockIdMustExist()
    {
        // Arrange
        var nonExistentFlockId = Guid.NewGuid();
        var record = DailyRecord.Create(_tenantId, nonExistentFlockId, DateTime.UtcNow, 10, null);
        _dbContext.DailyRecords.Add(record);

        // Act & Assert
        var act = async () => await _dbContext.SaveChangesAsync();
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task DailyRecord_CascadeDelete_DeletedWhenFlockIsDeleted()
    {
        // Arrange
        var record = DailyRecord.Create(_tenantId, _flockId, DateTime.UtcNow, 10, null);
        _dbContext.DailyRecords.Add(record);
        await _dbContext.SaveChangesAsync();

        var recordId = record.Id;

        // Act - Delete the flock (should cascade to daily records)
        var flock = await _dbContext.Flocks.FindAsync(_flockId);
        _dbContext.Flocks.Remove(flock!);
        await _dbContext.SaveChangesAsync();

        // Assert
        var deletedRecord = await _dbContext.DailyRecords.FindAsync(recordId);
        deletedRecord.Should().BeNull("CASCADE delete should remove daily record when flock is deleted");
    }

    #endregion

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Close();
        _connection.Dispose();
    }
}
