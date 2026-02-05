namespace ChickenTrack.Application.DTOs;

/// <summary>
/// Data Transfer Object for Clerk webhook payloads.
/// Represents the structure of webhook events from Clerk (user.created, user.updated).
/// </summary>
public sealed class ClerkWebhookDto
{
    /// <summary>
    /// Gets or sets the type of webhook event (e.g., "user.created", "user.updated").
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the webhook event data containing user information.
    /// </summary>
    public ClerkWebhookDataDto Data { get; set; } = new();
}

/// <summary>
/// Data Transfer Object for the data payload within a Clerk webhook.
/// Contains the user information from Clerk.
/// </summary>
public sealed class ClerkWebhookDataDto
{
    /// <summary>
    /// Gets or sets the unique Clerk user ID.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of email addresses associated with the user.
    /// </summary>
    public List<ClerkEmailDto> EmailAddresses { get; set; } = new();

    /// <summary>
    /// Gets or sets the primary email address ID.
    /// Used to identify which email in the EmailAddresses list is primary.
    /// </summary>
    public string? PrimaryEmailAddressId { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the user was created in Clerk (Unix milliseconds).
    /// </summary>
    public long CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the user was last updated in Clerk (Unix milliseconds).
    /// </summary>
    public long UpdatedAt { get; set; }
}

/// <summary>
/// Data Transfer Object for email address information from Clerk webhook.
/// </summary>
public sealed class ClerkEmailDto
{
    /// <summary>
    /// Gets or sets the unique identifier for this email address.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the email address.
    /// </summary>
    public string EmailAddress { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether this email address has been verified.
    /// </summary>
    public bool Verified { get; set; }
}
