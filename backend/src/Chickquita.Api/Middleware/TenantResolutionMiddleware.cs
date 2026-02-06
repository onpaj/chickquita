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
            // Extract Clerk user ID from JWT token
            var clerkUserId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? context.User.FindFirst("sub")?.Value;

            if (!string.IsNullOrEmpty(clerkUserId))
            {
                // Fetch tenant from database
                var tenant = await tenantRepository.GetByClerkUserIdAsync(clerkUserId);

                if (tenant == null)
                {
                    // Fallback behavior: Create tenant automatically if it doesn't exist
                    // This handles cases where the Clerk webhook didn't fire or failed
                    logger.LogWarning(
                        "Tenant not found for Clerk user ID: {ClerkUserId}. Creating tenant automatically (fallback).",
                        clerkUserId);

                    // Extract email from claims (Clerk includes this in the JWT)
                    var email = context.User.FindFirst(ClaimTypes.Email)?.Value
                                ?? context.User.FindFirst("email")?.Value
                                ?? $"{clerkUserId}@clerk.temp"; // Fallback email if not in claims

                    // Create new tenant
                    tenant = Tenant.Create(clerkUserId, email);
                    tenant = await tenantRepository.AddAsync(tenant);

                    logger.LogInformation(
                        "Auto-created tenant with ID: {TenantId} for Clerk user ID: {ClerkUserId}",
                        tenant.Id,
                        clerkUserId);
                }

                // Store tenant ID in HttpContext.Items for downstream use
                context.Items["TenantId"] = tenant.Id;
            }
        }

        // Call the next middleware in the pipeline
        await _next(context);
    }
}
