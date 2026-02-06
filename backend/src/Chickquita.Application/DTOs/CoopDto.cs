namespace Chickquita.Application.DTOs;

/// <summary>
/// Data Transfer Object for Coop entity.
/// </summary>
public sealed class CoopDto
{
    /// <summary>
    /// Unique identifier for the coop.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The tenant that owns this coop.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Name of the coop.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional location description for the coop.
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Timestamp when the coop was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when the coop was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
