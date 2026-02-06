namespace Chickquita.Application.Interfaces;

/// <summary>
/// Service for accessing the current tenant context
/// </summary>
public interface ITenantService
{
    /// <summary>
    /// Gets the current tenant ID from the HTTP context
    /// </summary>
    /// <returns>The tenant ID if available, otherwise null</returns>
    Guid? GetCurrentTenantId();
}
