namespace Chickquita.Domain.Entities;

/// <summary>
/// Represents a daily egg production record for a flock.
/// Used to track daily egg collection and calculate profitability.
/// </summary>
public class DailyRecord
{
    /// <summary>
    /// Unique identifier for the daily record.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// The tenant that owns this daily record.
    /// Used for multi-tenancy and data isolation.
    /// </summary>
    public Guid TenantId { get; private set; }

    /// <summary>
    /// The flock this record belongs to.
    /// </summary>
    public Guid FlockId { get; private set; }

    /// <summary>
    /// The date of the record (date only, time is ignored).
    /// Cannot be in the future.
    /// </summary>
    public DateTime RecordDate { get; private set; }

    /// <summary>
    /// Number of eggs collected on this date.
    /// Must be non-negative.
    /// </summary>
    public int EggCount { get; private set; }

    /// <summary>
    /// Optional notes about the daily collection (e.g., weather, incidents).
    /// Maximum 500 characters.
    /// </summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// Timestamp when the record was created.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Timestamp when the record was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    /// <summary>
    /// Navigation property to the flock.
    /// </summary>
    public Flock Flock { get; private set; } = null!;

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private DailyRecord()
    {
    }

    /// <summary>
    /// Factory method to create a new DailyRecord.
    /// </summary>
    /// <param name="tenantId">The ID of the tenant that owns this record</param>
    /// <param name="flockId">The ID of the flock this record belongs to</param>
    /// <param name="recordDate">The date of the record</param>
    /// <param name="eggCount">Number of eggs collected</param>
    /// <param name="notes">Optional notes about the collection</param>
    /// <returns>A new DailyRecord instance</returns>
    public static DailyRecord Create(
        Guid tenantId,
        Guid flockId,
        DateTime recordDate,
        int eggCount,
        string? notes = null)
    {
        // Validate tenant ID
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("Tenant ID cannot be empty.", nameof(tenantId));
        }

        // Validate flock ID
        if (flockId == Guid.Empty)
        {
            throw new ArgumentException("Flock ID cannot be empty.", nameof(flockId));
        }

        // Ensure recordDate is in UTC and normalize to date only (midnight)
        var recordDateUtc = recordDate.Kind switch
        {
            DateTimeKind.Utc => recordDate.Date,
            DateTimeKind.Local => recordDate.ToUniversalTime().Date,
            DateTimeKind.Unspecified => DateTime.SpecifyKind(recordDate.Date, DateTimeKind.Utc),
            _ => DateTime.SpecifyKind(recordDate.Date, DateTimeKind.Utc)
        };

        // Validate record date is not in the future
        if (recordDateUtc > DateTime.UtcNow.Date)
        {
            throw new ArgumentException("Record date cannot be in the future.", nameof(recordDate));
        }

        // Validate egg count is non-negative
        if (eggCount < 0)
        {
            throw new ArgumentException("Egg count cannot be negative.", nameof(eggCount));
        }

        // Validate notes length if provided
        if (notes != null && notes.Length > 500)
        {
            throw new ArgumentException("Notes cannot exceed 500 characters.", nameof(notes));
        }

        var now = DateTime.UtcNow;

        var dailyRecord = new DailyRecord
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            FlockId = flockId,
            RecordDate = recordDateUtc,
            EggCount = eggCount,
            Notes = notes,
            CreatedAt = now,
            UpdatedAt = now
        };

        return dailyRecord;
    }

    /// <summary>
    /// Updates the egg count and notes for this record.
    /// </summary>
    /// <param name="eggCount">New egg count</param>
    /// <param name="notes">Optional notes</param>
    public void Update(int eggCount, string? notes = null)
    {
        // Validate egg count is non-negative
        if (eggCount < 0)
        {
            throw new ArgumentException("Egg count cannot be negative.", nameof(eggCount));
        }

        // Validate notes length if provided
        if (notes != null && notes.Length > 500)
        {
            throw new ArgumentException("Notes cannot exceed 500 characters.", nameof(notes));
        }

        EggCount = eggCount;
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }
}
