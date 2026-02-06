using System.Net;
using System.Net.Http.Json;
using Chickquita.Application.DTOs;
using Chickquita.Application.Features.Coops.Commands;
using Chickquita.Application.Interfaces;
using Chickquita.Domain.Entities;
using Chickquita.Infrastructure.Data;
using Chickquita.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Chickquita.Api.Tests.Endpoints;

/// <summary>
/// Integration tests for Coops API endpoints.
/// Tests full HTTP flow including authentication, tenant isolation, and business logic.
/// </summary>
public class CoopsEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public CoopsEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateCoop_WithValidData_Returns201Created()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var mockCurrentUser = CreateMockCurrentUser("clerk_user_1", tenantId);

        using var scope = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                ReplaceWithInMemoryDatabase(services);
                ReplaceCurrentUserService(services, mockCurrentUser);
            });
        }).Services.CreateScope();

        await SeedTenant(scope, tenantId, "clerk_user_1");

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                ReplaceWithInMemoryDatabase(services);
                ReplaceCurrentUserService(services, mockCurrentUser);
            });
        }).CreateClient();

        var command = new CreateCoopCommand
        {
            Name = "Test Coop",
            Location = "Test Location"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/coops", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<CoopDto>();
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Coop");
        result.Location.Should().Be("Test Location");
        result.TenantId.Should().Be(tenantId);
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetCoops_WithExistingCoops_Returns200WithList()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var mockCurrentUser = CreateMockCurrentUser("clerk_user_1", tenantId);

        using var scope = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                ReplaceWithInMemoryDatabase(services);
                ReplaceCurrentUserService(services, mockCurrentUser);
            });
        }).Services.CreateScope();

        await SeedTenant(scope, tenantId, "clerk_user_1");
        await SeedCoop(scope, tenantId, "Coop 1", "Location 1");
        await SeedCoop(scope, tenantId, "Coop 2", "Location 2");

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                ReplaceWithInMemoryDatabase(services);
                ReplaceCurrentUserService(services, mockCurrentUser);
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/api/coops");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<CoopDto>>();
        result.Should().NotBeNull();
        result!.Should().Contain(c => c.Name == "Coop 1");
        result.Should().Contain(c => c.Name == "Coop 2");
    }

    [Fact]
    public async Task GetCoopById_WithExistingCoop_Returns200WithCoop()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var mockCurrentUser = CreateMockCurrentUser("clerk_user_1", tenantId);

        using var scope = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                ReplaceWithInMemoryDatabase(services);
                ReplaceCurrentUserService(services, mockCurrentUser);
            });
        }).Services.CreateScope();

        await SeedTenant(scope, tenantId, "clerk_user_1");
        var coopId = await SeedCoop(scope, tenantId, "Test Coop", "Test Location");

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                ReplaceWithInMemoryDatabase(services);
                ReplaceCurrentUserService(services, mockCurrentUser);
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync($"/api/coops/{coopId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<CoopDto>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(coopId);
        result.Name.Should().Be("Test Coop");
    }

    [Fact]
    public async Task GetCoopById_WithNonExistentCoop_Returns404NotFound()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var mockCurrentUser = CreateMockCurrentUser("clerk_user_1", tenantId);

        using var scope = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                ReplaceWithInMemoryDatabase(services);
                ReplaceCurrentUserService(services, mockCurrentUser);
            });
        }).Services.CreateScope();

        await SeedTenant(scope, tenantId, "clerk_user_1");

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                ReplaceWithInMemoryDatabase(services);
                ReplaceCurrentUserService(services, mockCurrentUser);
            });
        }).CreateClient();

        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await client.GetAsync($"/api/coops/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateCoop_WithValidData_Returns200WithUpdatedCoop()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var mockCurrentUser = CreateMockCurrentUser("clerk_user_1", tenantId);

        using var scope = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                ReplaceWithInMemoryDatabase(services);
                ReplaceCurrentUserService(services, mockCurrentUser);
            });
        }).Services.CreateScope();

        await SeedTenant(scope, tenantId, "clerk_user_1");
        var coopId = await SeedCoop(scope, tenantId, "Original Name", "Original Location");

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                ReplaceWithInMemoryDatabase(services);
                ReplaceCurrentUserService(services, mockCurrentUser);
            });
        }).CreateClient();

        var updateCommand = new UpdateCoopCommand
        {
            Id = coopId,
            Name = "Updated Name",
            Location = "Updated Location"
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/coops/{coopId}", updateCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<CoopDto>();
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task UpdateCoop_WithMismatchedIds_Returns400BadRequest()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var mockCurrentUser = CreateMockCurrentUser("clerk_user_1", tenantId);

        using var scope = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                ReplaceWithInMemoryDatabase(services);
                ReplaceCurrentUserService(services, mockCurrentUser);
            });
        }).Services.CreateScope();

        await SeedTenant(scope, tenantId, "clerk_user_1");
        var coopId = await SeedCoop(scope, tenantId, "Test Coop", "Test Location");

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                ReplaceWithInMemoryDatabase(services);
                ReplaceCurrentUserService(services, mockCurrentUser);
            });
        }).CreateClient();

        var updateCommand = new UpdateCoopCommand
        {
            Id = Guid.NewGuid(),
            Name = "Updated Name",
            Location = "Updated Location"
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/coops/{coopId}", updateCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteCoop_WithEmptyCoop_Returns200()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var mockCurrentUser = CreateMockCurrentUser("clerk_user_1", tenantId);

        using var scope = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                ReplaceWithInMemoryDatabase(services);
                ReplaceCurrentUserService(services, mockCurrentUser);
            });
        }).Services.CreateScope();

        await SeedTenant(scope, tenantId, "clerk_user_1");
        var coopId = await SeedCoop(scope, tenantId, "Test Coop", "Test Location");

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                ReplaceWithInMemoryDatabase(services);
                ReplaceCurrentUserService(services, mockCurrentUser);
            });
        }).CreateClient();

        // Act
        var response = await client.DeleteAsync($"/api/coops/{coopId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<bool>();
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteCoop_WithNonExistentCoop_Returns404NotFound()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var mockCurrentUser = CreateMockCurrentUser("clerk_user_1", tenantId);

        using var scope = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                ReplaceWithInMemoryDatabase(services);
                ReplaceCurrentUserService(services, mockCurrentUser);
            });
        }).Services.CreateScope();

        await SeedTenant(scope, tenantId, "clerk_user_1");

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                ReplaceWithInMemoryDatabase(services);
                ReplaceCurrentUserService(services, mockCurrentUser);
            });
        }).CreateClient();

        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await client.DeleteAsync($"/api/coops/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteCoop_WithFlocks_Returns400BadRequest()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var mockCurrentUser = CreateMockCurrentUser("clerk_user_1", tenantId);

        // Mock repository to return hasFlocks = true
        var mockCoopRepo = new Mock<ICoopRepository>();
        mockCoopRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(Coop.Create(tenantId, "Test", "Location"));
        mockCoopRepo.Setup(r => r.HasFlocksAsync(It.IsAny<Guid>()))
            .ReturnsAsync(true);

        using var scope = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                ReplaceWithInMemoryDatabase(services);
                ReplaceCurrentUserService(services, mockCurrentUser);
                ReplaceCoopRepository(services, mockCoopRepo.Object);
            });
        }).Services.CreateScope();

        await SeedTenant(scope, tenantId, "clerk_user_1");
        var coopId = Guid.NewGuid();

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                ReplaceWithInMemoryDatabase(services);
                ReplaceCurrentUserService(services, mockCurrentUser);
                ReplaceCoopRepository(services, mockCoopRepo.Object);
            });
        }).CreateClient();

        // Act
        var response = await client.DeleteAsync($"/api/coops/{coopId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task TenantIsolation_UserCannotSeeOtherTenantCoops()
    {
        // Arrange
        var tenant1Id = Guid.NewGuid();
        var tenant2Id = Guid.NewGuid();
        var mockCurrentUser1 = CreateMockCurrentUser("clerk_user_1", tenant1Id);

        using var scope = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                ReplaceWithInMemoryDatabase(services);
            });
        }).Services.CreateScope();

        await SeedTenant(scope, tenant1Id, "clerk_user_1");
        await SeedTenant(scope, tenant2Id, "clerk_user_2");
        await SeedCoop(scope, tenant1Id, "Tenant 1 Coop", "Location 1");
        await SeedCoop(scope, tenant2Id, "Tenant 2 Coop", "Location 2");

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                ReplaceWithInMemoryDatabase(services);
                ReplaceCurrentUserService(services, mockCurrentUser1);
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/api/coops");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<CoopDto>>();
        result.Should().NotBeNull();
        result!.Should().AllSatisfy(c => c.TenantId.Should().Be(tenant1Id));
        result.Should().NotContain(c => c.Name == "Tenant 2 Coop");
    }

    // Helper methods
    private static Mock<ICurrentUserService> CreateMockCurrentUser(string clerkUserId, Guid tenantId)
    {
        var mock = new Mock<ICurrentUserService>();
        mock.Setup(x => x.ClerkUserId).Returns(clerkUserId);
        mock.Setup(x => x.TenantId).Returns(tenantId);
        mock.Setup(x => x.IsAuthenticated).Returns(true);
        return mock;
    }

    private static void ReplaceWithInMemoryDatabase(IServiceCollection services)
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
        if (descriptor != null)
        {
            services.Remove(descriptor);
        }

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseInMemoryDatabase("TestDb");
        });

        // Bypass authentication for tests
        services.AddAuthorization(options =>
        {
            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAssertion(_ => true)
                .Build();
        });
    }

    private static void ReplaceCurrentUserService(IServiceCollection services, Mock<ICurrentUserService> mock)
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ICurrentUserService));
        if (descriptor != null)
        {
            services.Remove(descriptor);
        }

        services.AddScoped(_ => mock.Object);
    }

    private static void ReplaceCoopRepository(IServiceCollection services, ICoopRepository mockRepo)
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ICoopRepository));
        if (descriptor != null)
        {
            services.Remove(descriptor);
        }

        services.AddScoped(_ => mockRepo);
    }

    private static async Task SeedTenant(IServiceScope scope, Guid tenantId, string clerkUserId)
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var tenant = Tenant.Create(clerkUserId, $"{clerkUserId}@test.com");
        typeof(Tenant).GetProperty(nameof(Tenant.Id))!.SetValue(tenant, tenantId);
        dbContext.Tenants.Add(tenant);
        await dbContext.SaveChangesAsync();
    }

    private static async Task<Guid> SeedCoop(IServiceScope scope, Guid tenantId, string name, string location)
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var coop = Coop.Create(tenantId, name, location);
        dbContext.Coops.Add(coop);
        await dbContext.SaveChangesAsync();
        return coop.Id;
    }
}
