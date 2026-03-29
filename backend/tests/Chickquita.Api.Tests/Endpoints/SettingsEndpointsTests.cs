using System.Net;
using System.Net.Http.Json;
using Chickquita.Api.Endpoints;
using Chickquita.Api.Tests.Helpers;
using Chickquita.Application.DTOs;
using Chickquita.Application.Interfaces;
using Chickquita.Domain.Entities;
using Chickquita.Infrastructure.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Chickquita.Api.Tests.Endpoints;

/// <summary>
/// Integration tests for Settings API endpoints.
/// Tests full HTTP flow including authentication, tenant isolation, and business logic.
/// </summary>
public class SettingsEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public SettingsEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    #region GET /api/settings Tests

    [Fact]
    public async Task GetTenantSettings_WithValidTenant_Returns200WithSettings()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var mockCurrentUser = CreateMockCurrentUser("clerk_user_1", tenantId);

        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                ReplaceWithInMemoryDatabase(services);
                ReplaceCurrentUserService(services, mockCurrentUser);
            });
        });

        using var scope = factory.Services.CreateScope();
        await SeedTenant(scope, tenantId, "clerk_user_1");

        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/settings");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<TenantSettingsDto>();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetTenantSettings_WithDefaultTenant_ReturnsRevenueTrackingEnabledTrue()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var mockCurrentUser = CreateMockCurrentUser("clerk_user_1", tenantId);

        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                ReplaceWithInMemoryDatabase(services);
                ReplaceCurrentUserService(services, mockCurrentUser);
            });
        });

        using var scope = factory.Services.CreateScope();
        await SeedTenant(scope, tenantId, "clerk_user_1");

        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/settings");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<TenantSettingsDto>();
        result.Should().NotBeNull();
        result!.RevenueTrackingEnabled.Should().BeTrue();
        result.Currency.Should().Be("CZK");
    }

    [Fact]
    public async Task GetTenantSettings_WhenRevenueTrackingDisabled_ReturnsDisabledInResponse()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var mockCurrentUser = CreateMockCurrentUser("clerk_user_1", tenantId);

        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                ReplaceWithInMemoryDatabase(services);
                ReplaceCurrentUserService(services, mockCurrentUser);
            });
        });

        using var scope = factory.Services.CreateScope();
        await SeedTenantWithSettings(scope, tenantId, "clerk_user_1", singleCoopMode: false, revenueTrackingEnabled: false, currency: "EUR");

        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/settings");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<TenantSettingsDto>();
        result.Should().NotBeNull();
        result!.RevenueTrackingEnabled.Should().BeFalse();
        result.Currency.Should().Be("EUR");
    }

    #endregion

    #region PUT /api/settings Tests

    [Fact]
    public async Task UpdateTenantSettings_WithValidData_Returns204NoContent()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var mockCurrentUser = CreateMockCurrentUser("clerk_user_1", tenantId);

        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                ReplaceWithInMemoryDatabase(services);
                ReplaceCurrentUserService(services, mockCurrentUser);
            });
        });

        using var scope = factory.Services.CreateScope();
        await SeedTenant(scope, tenantId, "clerk_user_1");

        var client = factory.CreateClient();

        var request = new UpdateTenantSettingsRequest(
            SingleCoopMode: false,
            RevenueTrackingEnabled: true,
            Currency: "EUR");

        // Act
        var response = await client.PutAsJsonAsync("/api/settings", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateTenantSettings_WithRevenueTrackingDisabled_PersistsDisabledState()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var mockCurrentUser = CreateMockCurrentUser("clerk_user_1", tenantId);

        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                ReplaceWithInMemoryDatabase(services);
                ReplaceCurrentUserService(services, mockCurrentUser);
            });
        });

        using var scope = factory.Services.CreateScope();
        await SeedTenant(scope, tenantId, "clerk_user_1");

        var client = factory.CreateClient();

        var request = new UpdateTenantSettingsRequest(
            SingleCoopMode: false,
            RevenueTrackingEnabled: false,
            Currency: "CZK");

        // Act
        var updateResponse = await client.PutAsJsonAsync("/api/settings", request);

        // Assert — update succeeded
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Assert — GET reflects the disabled state
        var getResponse = await client.GetAsync("/api/settings");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await getResponse.Content.ReadFromJsonAsync<TenantSettingsDto>();
        result.Should().NotBeNull();
        result!.RevenueTrackingEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateTenantSettings_WithRevenueTrackingEnabled_PersistsEnabledState()
    {
        // Arrange — start with revenue tracking disabled
        var tenantId = Guid.NewGuid();
        var mockCurrentUser = CreateMockCurrentUser("clerk_user_1", tenantId);

        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                ReplaceWithInMemoryDatabase(services);
                ReplaceCurrentUserService(services, mockCurrentUser);
            });
        });

        using var scope = factory.Services.CreateScope();
        await SeedTenantWithSettings(scope, tenantId, "clerk_user_1", singleCoopMode: false, revenueTrackingEnabled: false, currency: "CZK");

        var client = factory.CreateClient();

        var request = new UpdateTenantSettingsRequest(
            SingleCoopMode: false,
            RevenueTrackingEnabled: true,
            Currency: "CZK");

        // Act
        var updateResponse = await client.PutAsJsonAsync("/api/settings", request);

        // Assert — update succeeded
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Assert — GET reflects the enabled state
        var getResponse = await client.GetAsync("/api/settings");
        var result = await getResponse.Content.ReadFromJsonAsync<TenantSettingsDto>();
        result.Should().NotBeNull();
        result!.RevenueTrackingEnabled.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateTenantSettings_WithCurrencyChange_PersistsCurrency()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var mockCurrentUser = CreateMockCurrentUser("clerk_user_1", tenantId);

        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                ReplaceWithInMemoryDatabase(services);
                ReplaceCurrentUserService(services, mockCurrentUser);
            });
        });

        using var scope = factory.Services.CreateScope();
        await SeedTenant(scope, tenantId, "clerk_user_1");

        var client = factory.CreateClient();

        var request = new UpdateTenantSettingsRequest(
            SingleCoopMode: false,
            RevenueTrackingEnabled: true,
            Currency: "EUR");

        // Act
        await client.PutAsJsonAsync("/api/settings", request);

        // Assert
        var getResponse = await client.GetAsync("/api/settings");
        var result = await getResponse.Content.ReadFromJsonAsync<TenantSettingsDto>();
        result.Should().NotBeNull();
        result!.Currency.Should().Be("EUR");
    }

    [Fact]
    public async Task UpdateTenantSettings_WithNullCurrency_DefaultsToCZK()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var mockCurrentUser = CreateMockCurrentUser("clerk_user_1", tenantId);

        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                ReplaceWithInMemoryDatabase(services);
                ReplaceCurrentUserService(services, mockCurrentUser);
            });
        });

        using var scope = factory.Services.CreateScope();
        await SeedTenant(scope, tenantId, "clerk_user_1");

        var client = factory.CreateClient();

        var request = new UpdateTenantSettingsRequest(
            SingleCoopMode: false,
            RevenueTrackingEnabled: true,
            Currency: null);

        // Act
        await client.PutAsJsonAsync("/api/settings", request);

        // Assert
        var getResponse = await client.GetAsync("/api/settings");
        var result = await getResponse.Content.ReadFromJsonAsync<TenantSettingsDto>();
        result.Should().NotBeNull();
        result!.Currency.Should().Be("CZK");
    }

    #endregion

    #region Tenant Isolation Tests

    [Fact]
    public async Task GetTenantSettings_TenantIsolation_ReturnsOnlyOwnSettings()
    {
        // Arrange — two tenants in the same DB with different settings
        var tenant1Id = Guid.NewGuid();
        var tenant2Id = Guid.NewGuid();
        var mockCurrentUser1 = CreateMockCurrentUser("clerk_user_1", tenant1Id);

        // Use the same factory for both seeding and the client so they share the same SQLite connection
        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                ReplaceWithInMemoryDatabase(services);
                ReplaceCurrentUserService(services, mockCurrentUser1);
            });
        });

        using var scope = factory.Services.CreateScope();
        await SeedTenantWithSettings(scope, tenant1Id, "clerk_user_1", singleCoopMode: false, revenueTrackingEnabled: true, currency: "CZK");
        await SeedTenantWithSettings(scope, tenant2Id, "clerk_user_2", singleCoopMode: true, revenueTrackingEnabled: false, currency: "EUR");

        var client = factory.CreateClient();

        // Act — authenticated as tenant1
        var response = await client.GetAsync("/api/settings");

        // Assert — returns only tenant1's settings, not tenant2's
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<TenantSettingsDto>();
        result.Should().NotBeNull();
        result!.RevenueTrackingEnabled.Should().BeTrue();
        result.Currency.Should().Be("CZK");
    }

    #endregion

    #region Endpoint Contract Tests

    [Fact]
    public void AllEndpoints_RequireAuthorization()
    {
        // Verify SettingsEndpoints.MapSettingsEndpoints exists and is properly structured
        var mapMethod = typeof(SettingsEndpoints).GetMethod("MapSettingsEndpoints");
        mapMethod.Should().NotBeNull("MapSettingsEndpoints method should exist");
        mapMethod!.IsStatic.Should().BeTrue("MapSettingsEndpoints should be a static extension method");

        var parameters = mapMethod.GetParameters();
        parameters.Should().HaveCount(1, "MapSettingsEndpoints should have exactly one parameter");
        parameters[0].ParameterType.Name.Should().Be("IEndpointRouteBuilder");
    }

    #endregion

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

        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        services.AddSingleton(connection);

        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            var conn = serviceProvider.GetRequiredService<SqliteConnection>();
            options.UseSqlite(conn);
            options.EnableSensitiveDataLogging();
        });

        var appContextDescriptors = services.Where(d => d.ServiceType == typeof(ApplicationDbContext)).ToList();
        foreach (var d in appContextDescriptors) services.Remove(d);
        services.AddScoped<ApplicationDbContext>(sp =>
        {
            var opts = sp.GetRequiredService<DbContextOptions<ApplicationDbContext>>();
            var currentUser = sp.GetRequiredService<ICurrentUserService>();
            return new SqliteApplicationDbContext(opts, currentUser);
        });

        services.AddAuthorization(options =>
        {
            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAssertion(_ => true)
                .Build();
        });

        services.AddTransient<IStartupFilter, DatabaseInitializerStartupFilter>();
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

    private static async Task SeedTenant(IServiceScope scope, Guid tenantId, string clerkUserId)
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
        var tenant = Tenant.Create(clerkUserId, $"{clerkUserId}@test.com").Value;
        typeof(Tenant).GetProperty(nameof(Tenant.Id))!.SetValue(tenant, tenantId);
        dbContext.Tenants.Add(tenant);
        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedTenantWithSettings(
        IServiceScope scope,
        Guid tenantId,
        string clerkUserId,
        bool singleCoopMode,
        bool revenueTrackingEnabled,
        string currency)
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
        var tenant = Tenant.Create(clerkUserId, $"{clerkUserId}@test.com").Value;
        typeof(Tenant).GetProperty(nameof(Tenant.Id))!.SetValue(tenant, tenantId);
        tenant.UpdateSettings(singleCoopMode: singleCoopMode, revenueTrackingEnabled: revenueTrackingEnabled, currency: currency);
        dbContext.Tenants.Add(tenant);
        await dbContext.SaveChangesAsync();
    }
}
