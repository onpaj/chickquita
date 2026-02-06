namespace Chickquita.Application.Interfaces;

/// <summary>
/// Service for accessing the current authenticated user's information
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the Clerk user ID of the current authenticated user
    /// </summary>
    /// <returns>The Clerk user ID, or null if not authenticated</returns>
    string? ClerkUserId { get; }

    /// <summary>
    /// Gets the tenant ID of the current authenticated user
    /// Set by TenantResolutionMiddleware from HttpContext.Items
    /// </summary>
    /// <returns>The tenant ID, or null if not authenticated</returns>
    Guid? TenantId { get; }

    /// <summary>
    /// Gets whether the current request is from an authenticated user
    /// </summary>
    bool IsAuthenticated { get; }
}
