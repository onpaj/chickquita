using Chickquita.Domain.Common;
using Chickquita.Domain.Entities;
using FluentAssertions;

namespace Chickquita.Domain.Tests.Entities;

/// <summary>
/// Unit tests for the Tenant domain entity.
/// Tests focus on domain logic, invariants, and validation rules for tenant management.
/// </summary>
public class TenantTests
{
    private const string ValidOrgId = "org_abc123";
    private const string ValidName = "Smith Farm";

    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        // Arrange & Act
        var result = Tenant.Create(ValidOrgId, ValidName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var tenant = result.Value;
        tenant.Should().NotBeNull();
        tenant.Id.Should().NotBeEmpty();
        tenant.ClerkOrgId.Should().Be(ValidOrgId);
        tenant.Name.Should().Be(ValidName);
        tenant.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        tenant.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        tenant.CreatedAt.Should().Be(tenant.UpdatedAt);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhitespaceOrgId_ShouldReturnFailure(string? invalidOrgId)
    {
        // Arrange & Act
        var result = Tenant.Create(invalidOrgId!, ValidName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Clerk org ID cannot be empty");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhitespaceName_ShouldReturnFailure(string? invalidName)
    {
        // Arrange & Act
        var result = Tenant.Create(ValidOrgId, invalidName!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Name cannot be empty");
    }

    [Fact]
    public void Create_ShouldGenerateUniqueIds()
    {
        // Arrange & Act
        var tenant1 = Tenant.Create(ValidOrgId, "Farm 1").Value;
        var tenant2 = Tenant.Create(ValidOrgId, "Farm 2").Value;

        // Assert
        tenant1.Id.Should().NotBe(tenant2.Id);
    }

    [Fact]
    public void UpdateName_WithValidName_ShouldUpdateNameAndTimestamp()
    {
        // Arrange
        var tenant = Tenant.Create(ValidOrgId, "Old Name").Value;
        var originalUpdatedAt = tenant.UpdatedAt;
        Thread.Sleep(10); // Ensure time difference

        // Act
        var updateResult = tenant.UpdateName("New Name");

        // Assert
        updateResult.IsSuccess.Should().BeTrue();
        tenant.Name.Should().Be("New Name");
        tenant.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateName_WithNullOrWhitespaceName_ShouldReturnFailure(string? invalidName)
    {
        // Arrange
        var tenant = Tenant.Create(ValidOrgId, ValidName).Value;

        // Act
        var updateResult = tenant.UpdateName(invalidName!);

        // Assert
        updateResult.IsFailure.Should().BeTrue();
        updateResult.Error.Message.Should().Contain("Name cannot be empty");
    }
}
