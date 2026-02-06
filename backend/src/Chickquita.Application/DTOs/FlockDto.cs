namespace Chickquita.Application.DTOs;

/// <summary>
/// Data Transfer Object for Flock entity.
/// </summary>
public sealed class FlockDto
{
    /// <summary>
    /// Unique identifier for the flock.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The tenant that owns this flock.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// The coop this flock belongs to.
    /// </summary>
    public Guid CoopId { get; set; }

    /// <summary>
    /// User-defined identifier for the flock (e.g., "Spring 2024", "Batch A").
    /// </summary>
    public string Identifier { get; set; } = string.Empty;

    /// <summary>
    /// The hatch date of the flock.
    /// </summary>
    public DateTime HatchDate { get; set; }

    /// <summary>
    /// Current number of hens in the flock.
    /// </summary>
    public int CurrentHens { get; set; }

    /// <summary>
    /// Current number of roosters in the flock.
    /// </summary>
    public int CurrentRoosters { get; set; }

    /// <summary>
    /// Current number of chicks in the flock.
    /// </summary>
    public int CurrentChicks { get; set; }

    /// <summary>
    /// Indicates whether the flock is currently active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Timestamp when the flock was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when the flock was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
