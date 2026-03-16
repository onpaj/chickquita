using System.Net;
using System.Security.Claims;
using Chickquita.Application.Interfaces;
using Chickquita.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Chickquita.Api.Tests.Middleware;

/// <summary>
/// Tests for TenantResolutionMiddleware.
/// Validates tenant resolution using Clerk org_id JWT claim.
/// </summary>
public class TenantResolutionMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WithValidClerkOrgId_ExtractsClerkOrgIdFromJWT()
    {
        // Arrange
        var clerkOrgId = "org_abc123";
        var tenant = Tenant.Create(clerkOrgId, "Smith Farm").Value;

        var mockTenantRepository = new Mock<ITenantRepository>();
        mockTenantRepository
            .Setup(r => r.GetByClerkOrgIdAsync(clerkOrgId))
            .ReturnsAsync(tenant);

        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("org_id", clerkOrgId)
        }, "TestAuth"));

        var requestDelegate = new Mock<RequestDelegate>();
        requestDelegate
            .Setup(rd => rd(It.IsAny<HttpContext>()))
            .Returns(Task.CompletedTask);

        var mockLogger = new Mock<ILogger<Chickquita.Api.Middleware.TenantResolutionMiddleware>>();

        var middleware = new Chickquita.Api.Middleware.TenantResolutionMiddleware(
            requestDelegate.Object);

        // Act
        await middleware.InvokeAsync(httpContext, mockTenantRepository.Object, mockLogger.Object);

        // Assert
        mockTenantRepository.Verify(
            r => r.GetByClerkOrgIdAsync(clerkOrgId),
            Times.Once,
            "Middleware should extract Clerk org ID from JWT and use it to fetch tenant");
    }

    [Fact]
    public async Task InvokeAsync_WithValidTenant_StoresTenantIdInHttpContextItems()
    {
        // Arrange
        var clerkOrgId = "org_abc123";
        var tenant = Tenant.Create(clerkOrgId, "Smith Farm").Value;

        var mockTenantRepository = new Mock<ITenantRepository>();
        mockTenantRepository
            .Setup(r => r.GetByClerkOrgIdAsync(clerkOrgId))
            .ReturnsAsync(tenant);

        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("org_id", clerkOrgId)
        }, "TestAuth"));

        var requestDelegate = new Mock<RequestDelegate>();
        requestDelegate
            .Setup(rd => rd(It.IsAny<HttpContext>()))
            .Returns(Task.CompletedTask);

        var mockLogger = new Mock<ILogger<Chickquita.Api.Middleware.TenantResolutionMiddleware>>();

        var middleware = new Chickquita.Api.Middleware.TenantResolutionMiddleware(
            requestDelegate.Object);

        // Act
        await middleware.InvokeAsync(httpContext, mockTenantRepository.Object, mockLogger.Object);

        // Assert
        httpContext.Items.Should().ContainKey("TenantId");
        httpContext.Items["TenantId"].Should().Be(tenant.Id);
    }

    [Fact]
    public async Task InvokeAsync_WhenTenantNotFound_AutoCreatesNewTenant()
    {
        // Arrange
        var clerkOrgId = "org_abc123";
        var orgName = "Smith Farm";
        var createdTenant = Tenant.Create(clerkOrgId, orgName).Value;

        var mockTenantRepository = new Mock<ITenantRepository>();
        mockTenantRepository
            .Setup(r => r.GetByClerkOrgIdAsync(clerkOrgId))
            .ReturnsAsync((Tenant?)null);
        mockTenantRepository
            .Setup(r => r.AddAsync(It.IsAny<Tenant>()))
            .ReturnsAsync(createdTenant);

        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("org_id", clerkOrgId),
            new Claim("org_name", orgName)
        }, "TestAuth"));

        var requestDelegate = new Mock<RequestDelegate>();
        requestDelegate
            .Setup(rd => rd(It.IsAny<HttpContext>()))
            .Returns(Task.CompletedTask);

        var mockLogger = new Mock<ILogger<Chickquita.Api.Middleware.TenantResolutionMiddleware>>();

        var middleware = new Chickquita.Api.Middleware.TenantResolutionMiddleware(
            requestDelegate.Object);

        // Act
        await middleware.InvokeAsync(httpContext, mockTenantRepository.Object, mockLogger.Object);

        // Assert
        mockTenantRepository.Verify(
            r => r.AddAsync(It.Is<Tenant>(t => t.ClerkOrgId == clerkOrgId && t.Name == orgName)),
            Times.Once,
            "Should auto-create tenant when not found");

        httpContext.Items.Should().ContainKey("TenantId");
        httpContext.Items["TenantId"].Should().Be(createdTenant.Id);

        requestDelegate.Verify(
            rd => rd(It.IsAny<HttpContext>()),
            Times.Once,
            "Should call next middleware after creating tenant");
    }

    [Fact]
    public async Task InvokeAsync_WithValidTenant_CallsNextMiddleware()
    {
        // Arrange
        var clerkOrgId = "org_abc123";
        var tenant = Tenant.Create(clerkOrgId, "Smith Farm").Value;

        var mockTenantRepository = new Mock<ITenantRepository>();
        mockTenantRepository
            .Setup(r => r.GetByClerkOrgIdAsync(clerkOrgId))
            .ReturnsAsync(tenant);

        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("org_id", clerkOrgId)
        }, "TestAuth"));

        var nextCalled = false;
        var requestDelegate = new Mock<RequestDelegate>();
        requestDelegate
            .Setup(rd => rd(It.IsAny<HttpContext>()))
            .Callback(() => nextCalled = true)
            .Returns(Task.CompletedTask);

        var mockLogger = new Mock<ILogger<Chickquita.Api.Middleware.TenantResolutionMiddleware>>();

        var middleware = new Chickquita.Api.Middleware.TenantResolutionMiddleware(
            requestDelegate.Object);

        // Act
        await middleware.InvokeAsync(httpContext, mockTenantRepository.Object, mockLogger.Object);

        // Assert
        nextCalled.Should().BeTrue("Middleware should call next delegate when tenant is found");
    }

    [Fact]
    public async Task InvokeAsync_WhenUserNotAuthenticated_CallsNextMiddlewareWithoutSettingTenant()
    {
        // Arrange
        var mockTenantRepository = new Mock<ITenantRepository>();

        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity()); // Not authenticated

        var nextCalled = false;
        var requestDelegate = new Mock<RequestDelegate>();
        requestDelegate
            .Setup(rd => rd(It.IsAny<HttpContext>()))
            .Callback(() => nextCalled = true)
            .Returns(Task.CompletedTask);

        var mockLogger = new Mock<ILogger<Chickquita.Api.Middleware.TenantResolutionMiddleware>>();

        var middleware = new Chickquita.Api.Middleware.TenantResolutionMiddleware(
            requestDelegate.Object);

        // Act
        await middleware.InvokeAsync(httpContext, mockTenantRepository.Object, mockLogger.Object);

        // Assert
        nextCalled.Should().BeTrue("Middleware should call next delegate for unauthenticated requests");
        httpContext.Items.Should().NotContainKey("TenantId");
        mockTenantRepository.Verify(
            r => r.GetByClerkOrgIdAsync(It.IsAny<string>()),
            Times.Never,
            "Should not query tenant repository when user is not authenticated");
    }

    [Fact]
    public async Task InvokeAsync_WithNoOrgIdClaim_DoesNotSetTenantId()
    {
        // Arrange
        var mockTenantRepository = new Mock<ITenantRepository>();

        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("sub", "user_xyz")  // authenticated but no org active
        }, "TestAuth"));

        var requestDelegate = new Mock<RequestDelegate>();
        requestDelegate
            .Setup(rd => rd(It.IsAny<HttpContext>()))
            .Returns(Task.CompletedTask);

        var mockLogger = new Mock<ILogger<Chickquita.Api.Middleware.TenantResolutionMiddleware>>();

        var middleware = new Chickquita.Api.Middleware.TenantResolutionMiddleware(
            requestDelegate.Object);

        // Act
        await middleware.InvokeAsync(httpContext, mockTenantRepository.Object, mockLogger.Object);

        // Assert
        httpContext.Items.Should().NotContainKey("TenantId",
            "TenantId should not be set when no org_id claim is present");
        mockTenantRepository.Verify(
            r => r.GetByClerkOrgIdAsync(It.IsAny<string>()),
            Times.Never,
            "Should not query tenant repository when org_id claim is missing");
    }

    [Fact]
    public async Task InvokeAsync_WhenTenantNotFound_AndNoOrgNameInClaims_UsesFallbackName()
    {
        // Arrange
        var clerkOrgId = "org_abc123";
        var expectedFallbackName = clerkOrgId; // Middleware falls back to org ID as name
        var createdTenant = Tenant.Create(clerkOrgId, expectedFallbackName).Value;

        var mockTenantRepository = new Mock<ITenantRepository>();
        mockTenantRepository
            .Setup(r => r.GetByClerkOrgIdAsync(clerkOrgId))
            .ReturnsAsync((Tenant?)null);
        mockTenantRepository
            .Setup(r => r.AddAsync(It.IsAny<Tenant>()))
            .ReturnsAsync(createdTenant);

        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("org_id", clerkOrgId)
            // No org_name claim
        }, "TestAuth"));

        var requestDelegate = new Mock<RequestDelegate>();
        requestDelegate
            .Setup(rd => rd(It.IsAny<HttpContext>()))
            .Returns(Task.CompletedTask);

        var mockLogger = new Mock<ILogger<Chickquita.Api.Middleware.TenantResolutionMiddleware>>();

        var middleware = new Chickquita.Api.Middleware.TenantResolutionMiddleware(
            requestDelegate.Object);

        // Act
        await middleware.InvokeAsync(httpContext, mockTenantRepository.Object, mockLogger.Object);

        // Assert
        mockTenantRepository.Verify(
            r => r.AddAsync(It.Is<Tenant>(t =>
                t.ClerkOrgId == clerkOrgId &&
                t.Name == expectedFallbackName)),
            Times.Once,
            "Should use fallback name (org ID) when org_name claim is not present");
    }
}
