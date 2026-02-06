namespace Chickquita.Application.DTOs;

/// <summary>
/// Data Transfer Object for FlockHistory entity.
/// </summary>
public sealed class FlockHistoryDto
{
    /// <summary>
    /// Unique identifier for the history entry.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The tenant that owns this history entry.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// The flock this history entry belongs to.
    /// </summary>
    public Guid FlockId { get; set; }

    /// <summary>
    /// The date when this composition change occurred.
    /// </summary>
    public DateTime ChangeDate { get; set; }

    /// <summary>
    /// Number of hens at this point in time.
    /// </summary>
    public int Hens { get; set; }

    /// <summary>
    /// Number of roosters at this point in time.
    /// </summary>
    public int Roosters { get; set; }

    /// <summary>
    /// Number of chicks at this point in time.
    /// </summary>
    public int Chicks { get; set; }

    /// <summary>
    /// Optional notes about this composition change.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Reason for the composition change.
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the history entry was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when the history entry was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
