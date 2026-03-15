using Chickquita.Domain.Common;

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
    /// Optional time of egg collection (UTC). Allows multiple records per day.
    /// </summary>
    public TimeSpan? CollectionTime { get; private set; }

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
    /// <param name="collectionTime">Optional time of collection</param>
    /// <returns>A Result containing the new DailyRecord instance, or a validation error</returns>
    public static Result<DailyRecord> Create(
        Guid tenantId,
        Guid flockId,
        DateTime recordDate,
        int eggCount,
        string? notes = null,
        TimeSpan? collectionTime = null)
    {
        if (tenantId == Guid.Empty)
            return Error.Validation("Tenant ID cannot be empty.");

        if (flockId == Guid.Empty)
            return Error.Validation("Flock ID cannot be empty.");

        // Ensure recordDate is in UTC and normalize to date only (midnight)
        var recordDateUtc = recordDate.Kind switch
        {
            DateTimeKind.Utc => recordDate.Date,
            DateTimeKind.Local => recordDate.ToUniversalTime().Date,
            DateTimeKind.Unspecified => DateTime.SpecifyKind(recordDate.Date, DateTimeKind.Utc),
            _ => DateTime.SpecifyKind(recordDate.Date, DateTimeKind.Utc)
        };

        if (recordDateUtc > DateTime.UtcNow.Date)
            return Error.Validation("Record date cannot be in the future.");

        if (eggCount < 0)
            return Error.Validation("Egg count cannot be negative.");

        if (notes != null && notes.Length > 500)
            return Error.Validation("Notes cannot exceed 500 characters.");

        var now = DateTime.UtcNow;

        return new DailyRecord
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            FlockId = flockId,
            RecordDate = recordDateUtc,
            EggCount = eggCount,
            Notes = notes,
            CollectionTime = collectionTime,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    /// <summary>
    /// Updates the egg count, notes, and collection time for this record.
    /// </summary>
    /// <param name="eggCount">New egg count</param>
    /// <param name="notes">Optional notes</param>
    /// <param name="collectionTime">Optional collection time; null preserves the existing value</param>
    /// <returns>A Result indicating success or a validation error</returns>
    public Result Update(int eggCount, string? notes = null, TimeSpan? collectionTime = null)
    {
        if (eggCount < 0)
            return Error.Validation("Egg count cannot be negative.");

        if (notes != null && notes.Length > 500)
            return Error.Validation("Notes cannot exceed 500 characters.");

        EggCount = eggCount;
        Notes = notes;
        if (collectionTime.HasValue)
        {
            CollectionTime = collectionTime;
        }
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }
}
