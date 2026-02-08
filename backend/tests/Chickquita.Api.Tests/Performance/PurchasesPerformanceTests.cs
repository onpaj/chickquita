using System.Diagnostics;
using System.Net.Http.Json;
using Chickquita.Application.Interfaces;
using Chickquita.Domain.Entities;
using Chickquita.Infrastructure.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Chickquita.Api.Tests.Performance;

/// <summary>
/// Performance tests for Purchases endpoints
/// Optional: GET /api/purchases should respond < 500ms for 1000 records
/// </summary>
[Collection("PerformanceTests")]
public class PurchasesPerformanceTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public PurchasesPerformanceTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact(Skip = "Performance test - run manually when needed")]
    public async Task GetPurchases_WithLargeDataset_ShouldRespondWithin500ms()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var mockCurrentUser = CreateMockCurrentUser("clerk_perf_user", tenantId);

        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                ReplaceWithInMemoryDatabase(services);
                ReplaceCurrentUserService(services, mockCurrentUser);
            });
        });

        using var scope = factory.Services.CreateScope();
        await SeedTenant(scope, tenantId, "clerk_perf_user");

        // Seed 1000 purchase records
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        for (int i = 0; i < 1000; i++)
        {
            var purchase = Purchase.Create(
                tenantId,
                $"Test Purchase {i}",
                PurchaseType.Feed,
                100.00m + i,
                100.0m,
                QuantityUnit.Kg,
                DateTime.UtcNow.AddDays(-i),
                null,
                null,
                $"Supplier {i % 10}"
            );
            dbContext.Purchases.Add(purchase);
        }
        await dbContext.SaveChangesAsync();

        var client = factory.CreateClient();
        const int expectedMaxResponseTimeMs = 500;

        // Act
        var stopwatch = Stopwatch.StartNew();
        var response = await client.GetAsync("/api/v1/purchases");
        stopwatch.Stop();

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(
            expectedMaxResponseTimeMs,
            $"GET /api/purchases should respond within {expectedMaxResponseTimeMs}ms for 1000 records");
    }

    [Fact(Skip = "Performance test - run manually when needed")]
    public async Task GetPurchases_WithPagination_ShouldRespondWithin200ms()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var mockCurrentUser = CreateMockCurrentUser("clerk_perf_user_2", tenantId);

        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                ReplaceWithInMemoryDatabase(services);
                ReplaceCurrentUserService(services, mockCurrentUser);
            });
        });

        using var scope = factory.Services.CreateScope();
        await SeedTenant(scope, tenantId, "clerk_perf_user_2");

        var client = factory.CreateClient();
        const int expectedMaxResponseTimeMs = 200;

        // Act
        var stopwatch = Stopwatch.StartNew();
        var response = await client.GetAsync("/api/v1/purchases?pageSize=20&pageNumber=1");
        stopwatch.Stop();

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(
            expectedMaxResponseTimeMs,
            $"GET /api/purchases with pagination should respond within {expectedMaxResponseTimeMs}ms");
    }

    [Fact(Skip = "Performance test - run manually when needed")]
    public async Task CreatePurchase_ShouldRespondWithin300ms()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var mockCurrentUser = CreateMockCurrentUser("clerk_perf_user_3", tenantId);

        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                ReplaceWithInMemoryDatabase(services);
                ReplaceCurrentUserService(services, mockCurrentUser);
            });
        });

        using var scope = factory.Services.CreateScope();
        await SeedTenant(scope, tenantId, "clerk_perf_user_3");

        var client = factory.CreateClient();
        const int expectedMaxResponseTimeMs = 300;
        var purchaseData = new
        {
            name = "Performance Test Purchase",
            type = PurchaseType.Feed,
            amount = 250.00m,
            quantity = 100.0m,
            unit = QuantityUnit.Kg,
            purchaseDate = DateTime.UtcNow,
            supplier = "Test Supplier"
        };

        // Act
        var stopwatch = Stopwatch.StartNew();
        var response = await client.PostAsJsonAsync("/api/v1/purchases", purchaseData);
        stopwatch.Stop();

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(
            expectedMaxResponseTimeMs,
            $"POST /api/purchases should respond within {expectedMaxResponseTimeMs}ms");
    }

    [Fact(Skip = "Performance test - run manually when needed")]
    public async Task GetPurchaseNames_ForAutocomplete_ShouldRespondWithin200ms()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var mockCurrentUser = CreateMockCurrentUser("clerk_perf_user_4", tenantId);

        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                ReplaceWithInMemoryDatabase(services);
                ReplaceCurrentUserService(services, mockCurrentUser);
            });
        });

        using var scope = factory.Services.CreateScope();
        await SeedTenant(scope, tenantId, "clerk_perf_user_4");

        var client = factory.CreateClient();
        const int expectedMaxResponseTimeMs = 200;

        // Act
        var stopwatch = Stopwatch.StartNew();
        var response = await client.GetAsync("/api/v1/purchases/names?search=feed");
        stopwatch.Stop();

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(
            expectedMaxResponseTimeMs,
            $"GET /api/purchases/names should respond within {expectedMaxResponseTimeMs}ms for autocomplete");
    }

    #region Helper Methods

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

        var databaseName = $"TestDb_{Guid.NewGuid()}";

        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            options.UseInMemoryDatabase(databaseName);
            options.EnableSensitiveDataLogging();
            options.ConfigureWarnings(warnings => warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.CoreEventId.QueryIterationFailed));
        });

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

    private static async Task SeedTenant(IServiceScope scope, Guid tenantId, string clerkUserId)
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var tenant = Tenant.Create(clerkUserId, $"{clerkUserId}@test.com");
        typeof(Tenant).GetProperty(nameof(Tenant.Id))!.SetValue(tenant, tenantId);
        dbContext.Tenants.Add(tenant);
        await dbContext.SaveChangesAsync();
    }

    #endregion
}
