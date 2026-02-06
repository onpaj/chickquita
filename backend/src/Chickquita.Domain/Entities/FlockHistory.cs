namespace Chickquita.Domain.Entities;

/// <summary>
/// Represents an immutable history entry for flock composition changes.
/// Each entry records the flock composition at a specific point in time.
/// </summary>
public class FlockHistory
{
    /// <summary>
    /// Unique identifier for the history entry.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// The tenant that owns this history entry.
    /// Used for multi-tenancy and data isolation.
    /// </summary>
    public Guid TenantId { get; private set; }

    /// <summary>
    /// The flock this history entry belongs to.
    /// </summary>
    public Guid FlockId { get; private set; }

    /// <summary>
    /// The date when this composition change occurred.
    /// </summary>
    public DateTime ChangeDate { get; private set; }

    /// <summary>
    /// Number of hens at this point in time.
    /// </summary>
    public int Hens { get; private set; }

    /// <summary>
    /// Number of roosters at this point in time.
    /// </summary>
    public int Roosters { get; private set; }

    /// <summary>
    /// Number of chicks at this point in time.
    /// </summary>
    public int Chicks { get; private set; }

    /// <summary>
    /// Optional notes about this composition change.
    /// This is the only mutable field in history entries.
    /// </summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// Reason for the composition change.
    /// Examples: "Initial", "Maturation", "Purchase", "Death", "Sale"
    /// </summary>
    public string Reason { get; private set; } = string.Empty;

    /// <summary>
    /// Timestamp when the history entry was created.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Timestamp when the history entry was last updated.
    /// Only Notes can be updated after creation.
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    /// <summary>
    /// Navigation property to the flock.
    /// </summary>
    public Flock Flock { get; private set; } = null!;

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private FlockHistory()
    {
    }

    /// <summary>
    /// Factory method to create a new FlockHistory entry.
    /// </summary>
    /// <param name="tenantId">The ID of the tenant</param>
    /// <param name="flockId">The ID of the flock</param>
    /// <param name="changeDate">The date of the composition change</param>
    /// <param name="hens">Number of hens</param>
    /// <param name="roosters">Number of roosters</param>
    /// <param name="chicks">Number of chicks</param>
    /// <param name="reason">Reason for the change</param>
    /// <param name="notes">Optional notes</param>
    /// <returns>A new FlockHistory instance</returns>
    public static FlockHistory Create(
        Guid tenantId,
        Guid flockId,
        DateTime changeDate,
        int hens,
        int roosters,
        int chicks,
        string reason,
        string? notes = null)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("Tenant ID cannot be empty.", nameof(tenantId));
        }

        if (flockId == Guid.Empty)
        {
            throw new ArgumentException("Flock ID cannot be empty.", nameof(flockId));
        }

        if (hens < 0)
        {
            throw new ArgumentException("Hens count cannot be negative.", nameof(hens));
        }

        if (roosters < 0)
        {
            throw new ArgumentException("Roosters count cannot be negative.", nameof(roosters));
        }

        if (chicks < 0)
        {
            throw new ArgumentException("Chicks count cannot be negative.", nameof(chicks));
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("Reason cannot be empty.", nameof(reason));
        }

        if (reason.Length > 50)
        {
            throw new ArgumentException("Reason cannot exceed 50 characters.", nameof(reason));
        }

        if (notes?.Length > 500)
        {
            throw new ArgumentException("Notes cannot exceed 500 characters.", nameof(notes));
        }

        var now = DateTime.UtcNow;

        return new FlockHistory
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            FlockId = flockId,
            ChangeDate = changeDate,
            Hens = hens,
            Roosters = roosters,
            Chicks = chicks,
            Reason = reason,
            Notes = notes,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    /// <summary>
    /// Updates the notes for this history entry.
    /// This is the only field that can be modified after creation.
    /// </summary>
    /// <param name="notes">The new notes</param>
    public void UpdateNotes(string? notes)
    {
        if (notes?.Length > 500)
        {
            throw new ArgumentException("Notes cannot exceed 500 characters.", nameof(notes));
        }

        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }
}
