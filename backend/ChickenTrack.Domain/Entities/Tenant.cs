namespace ChickenTrack.Domain.Entities;

/// <summary>
/// Represents a tenant (user account) in the system.
/// Each tenant has isolated data enforced by Row-Level Security (RLS).
/// Linked to Clerk user for authentication.
/// </summary>
public class Tenant
{
    /// <summary>
    /// Unique identifier for the tenant.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Clerk user ID for authentication integration.
    /// This links the tenant to their Clerk account.
    /// </summary>
    public string ClerkUserId { get; private set; } = string.Empty;

    /// <summary>
    /// Email address of the tenant.
    /// Synchronized from Clerk on user creation.
    /// </summary>
    public string Email { get; private set; } = string.Empty;

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
    private Tenant()
    {
    }

    /// <summary>
    /// Factory method to create a new Tenant.
    /// </summary>
    /// <param name="clerkUserId">The Clerk user ID</param>
    /// <param name="email">The tenant's email address</param>
    /// <returns>A new Tenant instance</returns>
    public static Tenant Create(string clerkUserId, string email)
    {
        if (string.IsNullOrWhiteSpace(clerkUserId))
        {
            throw new ArgumentException("Clerk user ID cannot be empty.", nameof(clerkUserId));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email cannot be empty.", nameof(email));
        }

        var now = DateTime.UtcNow;

        return new Tenant
        {
            Id = Guid.NewGuid(),
            ClerkUserId = clerkUserId,
            Email = email,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    /// <summary>
    /// Updates the tenant's email address.
    /// </summary>
    /// <param name="email">The new email address</param>
    public void UpdateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email cannot be empty.", nameof(email));
        }

        Email = email;
        UpdatedAt = DateTime.UtcNow;
    }
}
