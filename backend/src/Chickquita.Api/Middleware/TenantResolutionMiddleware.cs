using Chickquita.Application.Interfaces;
using Chickquita.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Chickquita.Api.Middleware;

/// <summary>
/// Middleware that resolves the current tenant from the JWT token.
/// Extracts the Clerk user ID from the JWT and fetches the corresponding tenant.
/// If no tenant exists, automatically creates one (fallback behavior).
/// Stores the tenant ID in HttpContext.Items for downstream use.
/// </summary>
public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantRepository tenantRepository, ILogger<TenantResolutionMiddleware> logger)
    {
        // Skip tenant resolution if user is not authenticated
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            // TODO: Task 5 - Read org_id claim from JWT instead of sub claim.
            // For now, use the org_id claim directly to look up the tenant.
            var clerkOrgId = context.User.FindFirst("org_id")?.Value;

            if (!string.IsNullOrEmpty(clerkOrgId))
            {
                // Fetch tenant from database using Clerk org ID
                var tenant = await tenantRepository.GetByClerkOrgIdAsync(clerkOrgId);

                if (tenant == null)
                {
                    // Fallback behavior: Create tenant automatically if it doesn't exist
                    // This handles cases where the Clerk webhook didn't fire or failed
                    logger.LogWarning(
                        "Tenant not found for Clerk org ID: {ClerkOrgId}. Creating tenant automatically (fallback).",
                        clerkOrgId);

                    // Extract org name from claims
                    var orgName = context.User.FindFirst("org_name")?.Value
                                  ?? clerkOrgId; // Fallback to org ID if name not in claims

                    // Create new tenant
                    tenant = Tenant.Create(clerkOrgId, orgName);
                    tenant = await tenantRepository.AddAsync(tenant);

                    logger.LogInformation(
                        "Auto-created tenant with ID: {TenantId} for Clerk org ID: {ClerkOrgId}",
                        tenant.Id,
                        clerkOrgId);
                }

                // Store tenant ID in HttpContext.Items for downstream use
                context.Items["TenantId"] = tenant.Id;
            }
        }

        // Call the next middleware in the pipeline
        await _next(context);
    }
}
