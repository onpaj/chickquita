using Chickquita.Domain.Common;

namespace Chickquita.Domain.Entities;

/// <summary>
/// Represents a tenant (organization / farm) in the system.
/// Each tenant has isolated data enforced by Row-Level Security (RLS).
/// Linked to a Clerk Organization for authentication.
/// </summary>
public class Tenant
{
    /// <summary>
    /// Unique identifier for the tenant.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Clerk Organization ID for authentication integration.
    /// This links the tenant to their Clerk organization.
    /// </summary>
    public string ClerkOrgId { get; private set; } = string.Empty;

    /// <summary>
    /// Display name of the organization / farm.
    /// Synchronized from Clerk on organization creation.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Timestamp when the tenant was created.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Timestamp when the tenant was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private Tenant() { }

    /// <summary>
    /// Factory method to create a new Tenant.
    /// </summary>
    /// <param name="clerkOrgId">The Clerk Organization ID</param>
    /// <param name="name">The display name of the organization / farm</param>
    /// <returns>A new Tenant instance</returns>
    public static Tenant Create(string clerkOrgId, string name)
    {
        if (string.IsNullOrWhiteSpace(clerkOrgId))
            throw new DomainValidationException("Clerk org ID cannot be empty.");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainValidationException("Name cannot be empty.");

        var now = DateTime.UtcNow;
        return new Tenant
        {
            Id = Guid.NewGuid(),
            ClerkOrgId = clerkOrgId,
            Name = name,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    /// <summary>
    /// Updates the tenant's display name.
    /// </summary>
    /// <param name="name">The new display name</param>
    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainValidationException("Name cannot be empty.");

        Name = name;
        UpdatedAt = DateTime.UtcNow;
    }
}
