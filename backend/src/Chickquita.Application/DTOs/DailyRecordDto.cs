namespace Chickquita.Application.DTOs;

/// <summary>
/// Data Transfer Object for DailyRecord entity.
/// </summary>
public sealed class DailyRecordDto
{
    /// <summary>
    /// Unique identifier for the daily record.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The tenant that owns this daily record.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// The flock this record belongs to.
    /// </summary>
    public Guid FlockId { get; set; }

    /// <summary>
    /// The date of the record (date only, time is ignored).
    /// </summary>
    public DateTime RecordDate { get; set; }

    /// <summary>
    /// Number of eggs collected on this date.
    /// </summary>
    public int EggCount { get; set; }

    /// <summary>
    /// Optional notes about the daily collection.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Timestamp when the record was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when the record was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
