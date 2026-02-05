using System.Net;
using System.Security.Claims;
using ChickenTrack.Application.Interfaces;
using ChickenTrack.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace ChickenTrack.Api.Tests.Middleware;

/// <summary>
/// Tests for TenantResolutionMiddleware.
/// Following TDD principles: Write tests first, watch them fail, then implement.
/// </summary>
public class TenantResolutionMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WithValidClerkUserId_ExtractsClerkUserIdFromJWT()
    {
        // Arrange
        var clerkUserId = "user_123";
        var tenantId = Guid.NewGuid();
        var tenant = Tenant.Create(clerkUserId, "test@example.com");

        var mockTenantRepository = new Mock<ITenantRepository>();
        mockTenantRepository
            .Setup(r => r.GetByClerkUserIdAsync(clerkUserId))
            .ReturnsAsync(tenant);

        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("sub", clerkUserId)
        }, "TestAuth"));

        var requestDelegate = new Mock<RequestDelegate>();
        requestDelegate
            .Setup(rd => rd(It.IsAny<HttpContext>()))
            .Returns(Task.CompletedTask);

        var middleware = new ChickenTrack.Api.Middleware.TenantResolutionMiddleware(
            requestDelegate.Object);

        // Act
        await middleware.InvokeAsync(httpContext, mockTenantRepository.Object);

        // Assert
        mockTenantRepository.Verify(
            r => r.GetByClerkUserIdAsync(clerkUserId),
            Times.Once,
            "Middleware should extract Clerk user ID from JWT and use it to fetch tenant");
    }

    [Fact]
    public async Task InvokeAsync_WithValidTenant_StoresTenantIdInHttpContextItems()
    {
        // Arrange
        var clerkUserId = "user_123";
        var tenant = Tenant.Create(clerkUserId, "test@example.com");

        var mockTenantRepository = new Mock<ITenantRepository>();
        mockTenantRepository
            .Setup(r => r.GetByClerkUserIdAsync(clerkUserId))
            .ReturnsAsync(tenant);

        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("sub", clerkUserId)
        }, "TestAuth"));

        var requestDelegate = new Mock<RequestDelegate>();
        requestDelegate
            .Setup(rd => rd(It.IsAny<HttpContext>()))
            .Returns(Task.CompletedTask);

        var middleware = new ChickenTrack.Api.Middleware.TenantResolutionMiddleware(
            requestDelegate.Object);

        // Act
        await middleware.InvokeAsync(httpContext, mockTenantRepository.Object);

        // Assert
        httpContext.Items.Should().ContainKey("TenantId");
        httpContext.Items["TenantId"].Should().Be(tenant.Id);
    }

    [Fact]
    public async Task InvokeAsync_WhenTenantNotFound_Returns403Forbidden()
    {
        // Arrange
        var clerkUserId = "user_123";

        var mockTenantRepository = new Mock<ITenantRepository>();
        mockTenantRepository
            .Setup(r => r.GetByClerkUserIdAsync(clerkUserId))
            .ReturnsAsync((Tenant?)null);

        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("sub", clerkUserId)
        }, "TestAuth"));
        httpContext.Response.Body = new MemoryStream();

        var requestDelegate = new Mock<RequestDelegate>();

        var middleware = new ChickenTrack.Api.Middleware.TenantResolutionMiddleware(
            requestDelegate.Object);

        // Act
        await middleware.InvokeAsync(httpContext, mockTenantRepository.Object);

        // Assert
        httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.Forbidden);
        requestDelegate.Verify(
            rd => rd(It.IsAny<HttpContext>()),
            Times.Never,
            "Should not call next middleware when tenant not found");
    }

    [Fact]
    public async Task InvokeAsync_WithValidTenant_CallsNextMiddleware()
    {
        // Arrange
        var clerkUserId = "user_123";
        var tenant = Tenant.Create(clerkUserId, "test@example.com");

        var mockTenantRepository = new Mock<ITenantRepository>();
        mockTenantRepository
            .Setup(r => r.GetByClerkUserIdAsync(clerkUserId))
            .ReturnsAsync(tenant);

        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("sub", clerkUserId)
        }, "TestAuth"));

        var nextCalled = false;
        var requestDelegate = new Mock<RequestDelegate>();
        requestDelegate
            .Setup(rd => rd(It.IsAny<HttpContext>()))
            .Callback(() => nextCalled = true)
            .Returns(Task.CompletedTask);

        var middleware = new ChickenTrack.Api.Middleware.TenantResolutionMiddleware(
            requestDelegate.Object);

        // Act
        await middleware.InvokeAsync(httpContext, mockTenantRepository.Object);

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

        var middleware = new ChickenTrack.Api.Middleware.TenantResolutionMiddleware(
            requestDelegate.Object);

        // Act
        await middleware.InvokeAsync(httpContext, mockTenantRepository.Object);

        // Assert
        nextCalled.Should().BeTrue("Middleware should call next delegate for unauthenticated requests");
        httpContext.Items.Should().NotContainKey("TenantId");
        mockTenantRepository.Verify(
            r => r.GetByClerkUserIdAsync(It.IsAny<string>()),
            Times.Never,
            "Should not query tenant repository when user is not authenticated");
    }
}
