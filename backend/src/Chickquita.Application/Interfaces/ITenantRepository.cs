using Chickquita.Domain.Entities;

namespace Chickquita.Application.Interfaces;

/// <summary>
/// Repository interface for Tenant entity operations
/// </summary>
public interface ITenantRepository
{
    /// <summary>
    /// Gets a tenant by their unique identifier
    /// </summary>
    /// <param name="id">The tenant ID</param>
    /// <returns>The tenant if found, otherwise null</returns>
    Task<Tenant?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets a tenant by their Clerk user ID
    /// </summary>
    /// <param name="clerkUserId">The Clerk user ID</param>
    /// <returns>The tenant if found, otherwise null</returns>
    Task<Tenant?> GetByClerkUserIdAsync(string clerkUserId);

    /// <summary>
    /// Gets a tenant by their email address
    /// </summary>
    /// <param name="email">The email address</param>
    /// <returns>The tenant if found, otherwise null</returns>
    Task<Tenant?> GetByEmailAsync(string email);

    /// <summary>
    /// Adds a new tenant to the repository
    /// </summary>
    /// <param name="tenant">The tenant to add</param>
    /// <returns>The added tenant</returns>
    Task<Tenant> AddAsync(Tenant tenant);

    /// <summary>
    /// Updates an existing tenant
    /// </summary>
    /// <param name="tenant">The tenant to update</param>
    Task UpdateAsync(Tenant tenant);

    /// <summary>
    /// Checks if a tenant exists with the given Clerk user ID
    /// </summary>
    /// <param name="clerkUserId">The Clerk user ID to check</param>
    /// <returns>True if exists, otherwise false</returns>
    Task<bool> ExistsByClerkUserIdAsync(string clerkUserId);
}
