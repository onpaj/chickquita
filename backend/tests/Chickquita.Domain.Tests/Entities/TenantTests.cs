using Chickquita.Domain.Entities;
using FluentAssertions;

namespace Chickquita.Domain.Tests.Entities;

public class TenantTests
{
    [Fact]
    public void Create_WithValidOrgId_ReturnsTenantWithCorrectProperties()
    {
        var orgId = "org_abc123";
        var name = "Smith Farm";

        var tenant = Tenant.Create(orgId, name);

        tenant.ClerkOrgId.Should().Be(orgId);
        tenant.Name.Should().Be(name);
        tenant.Id.Should().NotBeEmpty();
        tenant.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_WithEmptyOrgId_ThrowsArgumentException()
    {
        var act = () => Tenant.Create("", "Some Farm");
        act.Should().Throw<ArgumentException>().WithMessage("*org*");
    }

    [Fact]
    public void Create_WithEmptyName_ThrowsArgumentException()
    {
        var act = () => Tenant.Create("org_abc", "");
        act.Should().Throw<ArgumentException>().WithMessage("*name*");
    }

    [Fact]
    public void UpdateName_WithValidName_UpdatesNameAndTimestamp()
    {
        var tenant = Tenant.Create("org_abc", "Old Name");
        var before = tenant.UpdatedAt;

        tenant.UpdateName("New Name");

        tenant.Name.Should().Be("New Name");
        tenant.UpdatedAt.Should().BeOnOrAfter(before);
    }
}
