namespace ChickenTrack.Application.DTOs;

/// <summary>
/// Data Transfer Object for tenant information in API responses.
/// Maps to the Tenant domain entity.
/// </summary>
public sealed class TenantDto
{
    /// <summary>
    /// Gets or sets the unique identifier for the tenant.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the Clerk user ID for authentication integration.
    /// </summary>
    public string ClerkUserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the email address of the tenant.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp when the tenant was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the tenant was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
