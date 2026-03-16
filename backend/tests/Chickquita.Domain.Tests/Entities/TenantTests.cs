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
        var tenant = Tenant.Create(ValidOrgId, ValidName);

        // Assert
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
    public void Create_WithNullOrWhitespaceOrgId_ShouldThrowDomainValidationException(string? invalidOrgId)
    {
        // Arrange & Act
        var act = () => Tenant.Create(invalidOrgId!, ValidName);

        // Assert
        act.Should().Throw<DomainValidationException>()
            .WithMessage("Clerk org ID cannot be empty.*")
            .And.ParamName.Should().Be("clerkOrgId");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhitespaceName_ShouldThrowDomainValidationException(string? invalidName)
    {
        // Arrange & Act
        var act = () => Tenant.Create(ValidOrgId, invalidName!);

        // Assert
        act.Should().Throw<DomainValidationException>()
            .WithMessage("Name cannot be empty.*")
            .And.ParamName.Should().Be("name");
    }

    [Fact]
    public void Create_ShouldGenerateUniqueIds()
    {
        // Arrange & Act
        var tenant1 = Tenant.Create(ValidOrgId, "Farm 1");
        var tenant2 = Tenant.Create(ValidOrgId, "Farm 2");

        // Assert
        tenant1.Id.Should().NotBe(tenant2.Id);
    }

    [Fact]
    public void UpdateName_WithValidName_ShouldUpdateNameAndTimestamp()
    {
        // Arrange
        var tenant = Tenant.Create(ValidOrgId, "Old Name");
        var originalUpdatedAt = tenant.UpdatedAt;
        Thread.Sleep(10); // Ensure time difference

        // Act
        tenant.UpdateName("New Name");

        // Assert
        tenant.Name.Should().Be("New Name");
        tenant.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateName_WithNullOrWhitespaceName_ShouldThrowDomainValidationException(string? invalidName)
    {
        // Arrange
        var tenant = Tenant.Create(ValidOrgId, ValidName);

        // Act
        var act = () => tenant.UpdateName(invalidName!);

        // Assert
        act.Should().Throw<DomainValidationException>()
            .WithMessage("Name cannot be empty.*")
            .And.ParamName.Should().Be("name");
    }
}
