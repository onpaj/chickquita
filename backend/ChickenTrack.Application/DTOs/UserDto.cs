namespace ChickenTrack.Application.DTOs;

/// <summary>
/// Data Transfer Object for user/tenant information in API responses.
/// Maps to the Tenant domain entity.
/// </summary>
public sealed class UserDto
{
    /// <summary>
    /// Gets or sets the unique identifier for the user/tenant.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the Clerk user ID for authentication integration.
    /// </summary>
    public string ClerkUserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the email address of the user.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp when the user account was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the user account was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
