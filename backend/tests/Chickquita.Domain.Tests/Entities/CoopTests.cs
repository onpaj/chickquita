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
        var coop = Coop.Create(_validTenantId, ValidName, ValidLocation);

        // Assert
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
        var coop = Coop.Create(_validTenantId, ValidName);

        // Assert
        coop.Should().NotBeNull();
        coop.Name.Should().Be(ValidName);
        coop.Location.Should().BeNull();
        coop.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_WithEmptyTenantId_ShouldThrowArgumentException()
    {
        // Arrange
        var emptyTenantId = Guid.Empty;

        // Act
        var act = () => Coop.Create(emptyTenantId, ValidName);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Tenant ID cannot be empty.*")
            .And.ParamName.Should().Be("tenantId");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhitespaceName_ShouldThrowArgumentException(string? invalidName)
    {
        // Arrange & Act
        var act = () => Coop.Create(_validTenantId, invalidName!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Coop name cannot be empty.*")
            .And.ParamName.Should().Be("name");
    }

    [Fact]
    public void Create_WithNameExceeding100Characters_ShouldThrowArgumentException()
    {
        // Arrange
        var longName = new string('A', 101);

        // Act
        var act = () => Coop.Create(_validTenantId, longName);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Coop name cannot exceed 100 characters.*")
            .And.ParamName.Should().Be("name");
    }

    [Fact]
    public void Create_WithNameExactly100Characters_ShouldSucceed()
    {
        // Arrange
        var name = new string('A', 100);

        // Act
        var coop = Coop.Create(_validTenantId, name);

        // Assert
        coop.Should().NotBeNull();
        coop.Name.Should().Be(name);
        coop.Name.Length.Should().Be(100);
    }

    [Fact]
    public void Create_WithLocationExceeding200Characters_ShouldThrowArgumentException()
    {
        // Arrange
        var longLocation = new string('B', 201);

        // Act
        var act = () => Coop.Create(_validTenantId, ValidName, longLocation);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Location cannot exceed 200 characters.*")
            .And.ParamName.Should().Be("location");
    }

    [Fact]
    public void Create_WithLocationExactly200Characters_ShouldSucceed()
    {
        // Arrange
        var location = new string('B', 200);

        // Act
        var coop = Coop.Create(_validTenantId, ValidName, location);

        // Assert
        coop.Should().NotBeNull();
        coop.Location.Should().Be(location);
        coop.Location!.Length.Should().Be(200);
    }

    [Fact]
    public void Create_ShouldGenerateUniqueIds()
    {
        // Arrange & Act
        var coop1 = Coop.Create(_validTenantId, "Coop 1");
        var coop2 = Coop.Create(_validTenantId, "Coop 2");

        // Assert
        coop1.Id.Should().NotBe(coop2.Id);
    }

    #endregion

    #region Update Tests

    [Fact]
    public void Update_WithValidData_ShouldUpdateNameAndLocation()
    {
        // Arrange
        var coop = Coop.Create(_validTenantId, ValidName, ValidLocation);
        var originalCreatedAt = coop.CreatedAt;
        var originalUpdatedAt = coop.UpdatedAt;
        Thread.Sleep(10); // Ensure time difference

        var newName = "Updated Coop";
        var newLocation = "South Field";

        // Act
        coop.Update(newName, newLocation);

        // Assert
        coop.Name.Should().Be(newName);
        coop.Location.Should().Be(newLocation);
        coop.CreatedAt.Should().Be(originalCreatedAt);
        coop.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void Update_WithNullLocation_ShouldClearLocation()
    {
        // Arrange
        var coop = Coop.Create(_validTenantId, ValidName, ValidLocation);
        var newName = "Updated Coop";

        // Act
        coop.Update(newName, null);

        // Assert
        coop.Name.Should().Be(newName);
        coop.Location.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithNullOrWhitespaceName_ShouldThrowArgumentException(string? invalidName)
    {
        // Arrange
        var coop = Coop.Create(_validTenantId, ValidName);

        // Act
        var act = () => coop.Update(invalidName!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Coop name cannot be empty.*")
            .And.ParamName.Should().Be("name");
    }

    [Fact]
    public void Update_WithNameExceeding100Characters_ShouldThrowArgumentException()
    {
        // Arrange
        var coop = Coop.Create(_validTenantId, ValidName);
        var longName = new string('C', 101);

        // Act
        var act = () => coop.Update(longName);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Coop name cannot exceed 100 characters.*")
            .And.ParamName.Should().Be("name");
    }

    [Fact]
    public void Update_WithLocationExceeding200Characters_ShouldThrowArgumentException()
    {
        // Arrange
        var coop = Coop.Create(_validTenantId, ValidName);
        var longLocation = new string('D', 201);

        // Act
        var act = () => coop.Update(ValidName, longLocation);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Location cannot exceed 200 characters.*")
            .And.ParamName.Should().Be("location");
    }

    [Fact]
    public void Update_ShouldNotModifyOtherProperties()
    {
        // Arrange
        var coop = Coop.Create(_validTenantId, ValidName, ValidLocation);
        var originalId = coop.Id;
        var originalTenantId = coop.TenantId;
        var originalIsActive = coop.IsActive;
        var originalCreatedAt = coop.CreatedAt;

        // Act
        coop.Update("New Name", "New Location");

        // Assert
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
        var coop = Coop.Create(_validTenantId, ValidName);
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
        var coop = Coop.Create(_validTenantId, ValidName);
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
        var coop = Coop.Create(_validTenantId, ValidName, ValidLocation);
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
        var coop = Coop.Create(_validTenantId, ValidName);
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
        var coop = Coop.Create(_validTenantId, ValidName);
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
        var coop = Coop.Create(_validTenantId, ValidName, ValidLocation);
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
        var coop = Coop.Create(_validTenantId, ValidName, ValidLocation);
        var originalId = coop.Id;
        var originalTenantId = coop.TenantId;

        // Act - Update
        Thread.Sleep(10);
        coop.Update("Updated Name", "Updated Location");

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
        var coop = Coop.Create(_validTenantId, minimalName);

        // Assert
        coop.Should().NotBeNull();
        coop.Name.Should().Be(minimalName);
        coop.Location.Should().BeNull();
    }

    [Fact]
    public void Update_WithMinimalValidData_ShouldSucceed()
    {
        // Arrange
        var coop = Coop.Create(_validTenantId, ValidName);
        var minimalName = "B";

        // Act
        coop.Update(minimalName);

        // Assert
        coop.Name.Should().Be(minimalName);
    }

    [Fact]
    public void Create_WithSpecialCharactersInName_ShouldSucceed()
    {
        // Arrange
        var specialName = "Coop #1 - Main (Active)";

        // Act
        var coop = Coop.Create(_validTenantId, specialName);

        // Assert
        coop.Name.Should().Be(specialName);
    }

    [Fact]
    public void Create_WithUnicodeCharactersInName_ShouldSucceed()
    {
        // Arrange
        var unicodeName = "Kurník 🐔 Hlavní";

        // Act
        var coop = Coop.Create(_validTenantId, unicodeName);

        // Assert
        coop.Name.Should().Be(unicodeName);
    }

    #endregion
}
