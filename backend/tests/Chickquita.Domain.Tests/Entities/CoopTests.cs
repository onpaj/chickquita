using Chickquita.Domain.Common;
using Chickquita.Domain.Entities;
using FluentAssertions;

namespace Chickquita.Domain.Tests.Entities;

/// <summary>
/// Unit tests for the Coop domain entity.
/// Tests focus on domain logic, invariants, and validation rules.
/// </summary>
public class CoopTests
{
    private readonly Guid _validTenantId = Guid.NewGuid();
    private const string ValidName = "Main Coop";
    private const string ValidLocation = "North Field";

    #region Create Tests

    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        // Arrange & Act
        var result = Coop.Create(_validTenantId, ValidName, ValidLocation);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var coop = result.Value;
        coop.Should().NotBeNull();
        coop.Id.Should().NotBeEmpty();
        coop.TenantId.Should().Be(_validTenantId);
        coop.Name.Should().Be(ValidName);
        coop.Location.Should().Be(ValidLocation);
        coop.IsActive.Should().BeTrue();
        coop.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        coop.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        coop.CreatedAt.Should().Be(coop.UpdatedAt);
    }

    [Fact]
    public void Create_WithValidDataWithoutLocation_ShouldSucceed()
    {
        // Arrange & Act
        var result = Coop.Create(_validTenantId, ValidName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var coop = result.Value;
        coop.Should().NotBeNull();
        coop.Name.Should().Be(ValidName);
        coop.Location.Should().BeNull();
        coop.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_WithEmptyTenantId_ShouldReturnFailure()
    {
        // Arrange
        var emptyTenantId = Guid.Empty;

        // Act
        var result = Coop.Create(emptyTenantId, ValidName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Tenant ID cannot be empty");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhitespaceName_ShouldReturnFailure(string? invalidName)
    {
        // Arrange & Act
        var result = Coop.Create(_validTenantId, invalidName!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Coop name cannot be empty");
    }

    [Fact]
    public void Create_WithNameExceeding100Characters_ShouldReturnFailure()
    {
        // Arrange
        var longName = new string('A', 101);

        // Act
        var result = Coop.Create(_validTenantId, longName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Coop name cannot exceed 100 characters");
    }

    [Fact]
    public void Create_WithNameExactly100Characters_ShouldSucceed()
    {
        // Arrange
        var name = new string('A', 100);

        // Act
        var result = Coop.Create(_validTenantId, name);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var coop = result.Value;
        coop.Should().NotBeNull();
        coop.Name.Should().Be(name);
        coop.Name.Length.Should().Be(100);
    }

    [Fact]
    public void Create_WithLocationExceeding200Characters_ShouldReturnFailure()
    {
        // Arrange
        var longLocation = new string('B', 201);

        // Act
        var result = Coop.Create(_validTenantId, ValidName, longLocation);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Location cannot exceed 200 characters");
    }

    [Fact]
    public void Create_WithLocationExactly200Characters_ShouldSucceed()
    {
        // Arrange
        var location = new string('B', 200);

        // Act
        var result = Coop.Create(_validTenantId, ValidName, location);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var coop = result.Value;
        coop.Should().NotBeNull();
        coop.Location.Should().Be(location);
        coop.Location!.Length.Should().Be(200);
    }

    [Fact]
    public void Create_ShouldGenerateUniqueIds()
    {
        // Arrange & Act
        var coop1 = Coop.Create(_validTenantId, "Coop 1").Value;
        var coop2 = Coop.Create(_validTenantId, "Coop 2").Value;

        // Assert
        coop1.Id.Should().NotBe(coop2.Id);
    }

    #endregion

    #region Update Tests

    [Fact]
    public void Update_WithValidData_ShouldUpdateNameAndLocation()
    {
        // Arrange
        var coop = Coop.Create(_validTenantId, ValidName, ValidLocation).Value;
        var originalCreatedAt = coop.CreatedAt;
        var originalUpdatedAt = coop.UpdatedAt;
        Thread.Sleep(10); // Ensure time difference

        var newName = "Updated Coop";
        var newLocation = "South Field";

        // Act
        var updateResult = coop.Update(newName, newLocation);

        // Assert
        updateResult.IsSuccess.Should().BeTrue();
        coop.Name.Should().Be(newName);
        coop.Location.Should().Be(newLocation);
        coop.CreatedAt.Should().Be(originalCreatedAt);
        coop.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void Update_WithNullLocation_ShouldClearLocation()
    {
        // Arrange
        var coop = Coop.Create(_validTenantId, ValidName, ValidLocation).Value;
        var newName = "Updated Coop";

        // Act
        var updateResult = coop.Update(newName, null);

        // Assert
        updateResult.IsSuccess.Should().BeTrue();
        coop.Name.Should().Be(newName);
        coop.Location.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithNullOrWhitespaceName_ShouldReturnFailure(string? invalidName)
    {
        // Arrange
        var coop = Coop.Create(_validTenantId, ValidName).Value;

        // Act
        var updateResult = coop.Update(invalidName!);

        // Assert
        updateResult.IsFailure.Should().BeTrue();
        updateResult.Error.Message.Should().Contain("Coop name cannot be empty");
    }

    [Fact]
    public void Update_WithNameExceeding100Characters_ShouldReturnFailure()
    {
        // Arrange
        var coop = Coop.Create(_validTenantId, ValidName).Value;
        var longName = new string('C', 101);

        // Act
        var updateResult = coop.Update(longName);

        // Assert
        updateResult.IsFailure.Should().BeTrue();
        updateResult.Error.Message.Should().Contain("Coop name cannot exceed 100 characters");
    }

    [Fact]
    public void Update_WithLocationExceeding200Characters_ShouldReturnFailure()
    {
        // Arrange
        var coop = Coop.Create(_validTenantId, ValidName).Value;
        var longLocation = new string('D', 201);

        // Act
        var updateResult = coop.Update(ValidName, longLocation);

        // Assert
        updateResult.IsFailure.Should().BeTrue();
        updateResult.Error.Message.Should().Contain("Location cannot exceed 200 characters");
    }

    [Fact]
    public void Update_ShouldNotModifyOtherProperties()
    {
        // Arrange
        var coop = Coop.Create(_validTenantId, ValidName, ValidLocation).Value;
        var originalId = coop.Id;
        var originalTenantId = coop.TenantId;
        var originalIsActive = coop.IsActive;
        var originalCreatedAt = coop.CreatedAt;

        // Act
        var updateResult = coop.Update("New Name", "New Location");

        // Assert
        updateResult.IsSuccess.Should().BeTrue();
        coop.Id.Should().Be(originalId);
        coop.TenantId.Should().Be(originalTenantId);
        coop.IsActive.Should().Be(originalIsActive);
        coop.CreatedAt.Should().Be(originalCreatedAt);
    }

    #endregion

    #region Deactivate Tests

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var coop = Coop.Create(_validTenantId, ValidName).Value;
        var originalUpdatedAt = coop.UpdatedAt;
        Thread.Sleep(10); // Ensure time difference

        // Act
        coop.Deactivate();

        // Assert
        coop.IsActive.Should().BeFalse();
        coop.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void Deactivate_WhenAlreadyDeactivated_ShouldRemainDeactivated()
    {
        // Arrange
        var coop = Coop.Create(_validTenantId, ValidName).Value;
        coop.Deactivate();
        Thread.Sleep(10);
        var previousUpdatedAt = coop.UpdatedAt;

        // Act
        coop.Deactivate();

        // Assert
        coop.IsActive.Should().BeFalse();
        coop.UpdatedAt.Should().BeAfter(previousUpdatedAt);
    }

    [Fact]
    public void Deactivate_ShouldNotModifyOtherProperties()
    {
        // Arrange
        var coop = Coop.Create(_validTenantId, ValidName, ValidLocation).Value;
        var originalId = coop.Id;
        var originalTenantId = coop.TenantId;
        var originalName = coop.Name;
        var originalLocation = coop.Location;
        var originalCreatedAt = coop.CreatedAt;

        // Act
        coop.Deactivate();

        // Assert
        coop.Id.Should().Be(originalId);
        coop.TenantId.Should().Be(originalTenantId);
        coop.Name.Should().Be(originalName);
        coop.Location.Should().Be(originalLocation);
        coop.CreatedAt.Should().Be(originalCreatedAt);
    }

    #endregion

    #region Activate Tests

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var coop = Coop.Create(_validTenantId, ValidName).Value;
        coop.Deactivate();
        Thread.Sleep(10); // Ensure time difference
        var originalUpdatedAt = coop.UpdatedAt;

        // Act
        coop.Activate();

        // Assert
        coop.IsActive.Should().BeTrue();
        coop.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ShouldRemainActive()
    {
        // Arrange
        var coop = Coop.Create(_validTenantId, ValidName).Value;
        Thread.Sleep(10);
        var previousUpdatedAt = coop.UpdatedAt;

        // Act
        coop.Activate();

        // Assert
        coop.IsActive.Should().BeTrue();
        coop.UpdatedAt.Should().BeAfter(previousUpdatedAt);
    }

    [Fact]
    public void Activate_ShouldNotModifyOtherProperties()
    {
        // Arrange
        var coop = Coop.Create(_validTenantId, ValidName, ValidLocation).Value;
        coop.Deactivate();
        var originalId = coop.Id;
        var originalTenantId = coop.TenantId;
        var originalName = coop.Name;
        var originalLocation = coop.Location;
        var originalCreatedAt = coop.CreatedAt;

        // Act
        coop.Activate();

        // Assert
        coop.Id.Should().Be(originalId);
        coop.TenantId.Should().Be(originalTenantId);
        coop.Name.Should().Be(originalName);
        coop.Location.Should().Be(originalLocation);
        coop.CreatedAt.Should().Be(originalCreatedAt);
    }

    #endregion

    #region Lifecycle Tests

    [Fact]
    public void Lifecycle_CreateUpdateDeactivateActivate_ShouldMaintainConsistency()
    {
        // Arrange & Act - Create
        var coop = Coop.Create(_validTenantId, ValidName, ValidLocation).Value;
        var originalId = coop.Id;
        var originalTenantId = coop.TenantId;

        // Act - Update
        Thread.Sleep(10);
        var updateResult = coop.Update("Updated Name", "Updated Location");
        updateResult.IsSuccess.Should().BeTrue();

        // Act - Deactivate
        Thread.Sleep(10);
        coop.Deactivate();

        // Act - Activate
        Thread.Sleep(10);
        coop.Activate();

        // Assert - Verify full lifecycle
        coop.Id.Should().Be(originalId);
        coop.TenantId.Should().Be(originalTenantId);
        coop.Name.Should().Be("Updated Name");
        coop.Location.Should().Be("Updated Location");
        coop.IsActive.Should().BeTrue();
        coop.UpdatedAt.Should().BeAfter(coop.CreatedAt);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Create_WithMinimalValidData_ShouldSucceed()
    {
        // Arrange
        var minimalName = "A"; // Single character name

        // Act
        var result = Coop.Create(_validTenantId, minimalName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var coop = result.Value;
        coop.Should().NotBeNull();
        coop.Name.Should().Be(minimalName);
        coop.Location.Should().BeNull();
    }

    [Fact]
    public void Update_WithMinimalValidData_ShouldSucceed()
    {
        // Arrange
        var coop = Coop.Create(_validTenantId, ValidName).Value;
        var minimalName = "B";

        // Act
        var updateResult = coop.Update(minimalName);

        // Assert
        updateResult.IsSuccess.Should().BeTrue();
        coop.Name.Should().Be(minimalName);
    }

    [Fact]
    public void Create_WithSpecialCharactersInName_ShouldSucceed()
    {
        // Arrange
        var specialName = "Coop #1 - Main (Active)";

        // Act
        var result = Coop.Create(_validTenantId, specialName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(specialName);
    }

    [Fact]
    public void Create_WithUnicodeCharactersInName_ShouldSucceed()
    {
        // Arrange
        var unicodeName = "Kurník 🐔 Hlavní";

        // Act
        var result = Coop.Create(_validTenantId, unicodeName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(unicodeName);
    }

    #endregion
}
