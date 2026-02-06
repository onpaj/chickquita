using Chickquita.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Chickquita.Infrastructure.Services;

/// <summary>
/// Service for accessing the current tenant context from HTTP context
/// </summary>
public class TenantService : ITenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Gets the current tenant ID from the HTTP context items
    /// </summary>
    /// <returns>The tenant ID if available, otherwise null</returns>
    public Guid? GetCurrentTenantId()
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext == null)
        {
            return null;
        }

        if (httpContext.Items.TryGetValue("TenantId", out var tenantIdObj)
            && tenantIdObj is Guid tenantId)
        {
            return tenantId;
        }

        return null;
    }
}
