using Chickquita.Application.Interfaces;
using System.Security.Claims;

namespace Chickquita.Api.Middleware;

/// <summary>
/// Middleware that resolves the current tenant from the JWT token.
/// Extracts the Clerk user ID from the JWT and fetches the corresponding tenant.
/// Stores the tenant ID in HttpContext.Items for downstream use.
/// Returns 403 Forbidden if the user is authenticated but no tenant is found.
/// </summary>
public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantRepository tenantRepository)
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
                    // User is authenticated but no tenant exists - return 403 Forbidden
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        error = new
                        {
                            code = "TENANT_NOT_FOUND",
                            message = "Tenant not found for the authenticated user"
                        }
                    });
                    return;
                }

                // Store tenant ID in HttpContext.Items for downstream use
                context.Items["TenantId"] = tenant.Id;
            }
        }

        // Call the next middleware in the pipeline
        await _next(context);
    }
}
