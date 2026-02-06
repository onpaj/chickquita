using System.Security.Claims;
using Chickquita.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Chickquita.Infrastructure.Services;

/// <summary>
/// Service for accessing the current authenticated user's information from JWT claims
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Gets the Clerk user ID from the JWT token's subject claim
    /// Clerk uses the "sub" claim to store the user ID
    /// </summary>
    public string? ClerkUserId =>
        _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;

    /// <summary>
    /// Gets the tenant ID from HttpContext.Items
    /// Set by TenantResolutionMiddleware
    /// </summary>
    public Guid? TenantId
    {
        get
        {
            if (_httpContextAccessor.HttpContext?.Items.TryGetValue("TenantId", out var tenantIdObj) == true
                && tenantIdObj is Guid tenantId)
            {
                return tenantId;
            }
            return null;
        }
    }

    /// <summary>
    /// Gets whether the current request has an authenticated user
    /// </summary>
    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}
