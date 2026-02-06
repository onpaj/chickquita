namespace Chickquita.Domain.Entities;

/// <summary>
/// Represents a flock of chickens within a coop.
/// A flock tracks chicken composition (hens, roosters, chicks) over time.
/// </summary>
public class Flock
{
    /// <summary>
    /// Unique identifier for the flock.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// The tenant that owns this flock.
    /// Used for multi-tenancy and data isolation.
    /// </summary>
    public Guid TenantId { get; private set; }

    /// <summary>
    /// The coop this flock belongs to.
    /// </summary>
    public Guid CoopId { get; private set; }

    /// <summary>
    /// User-defined identifier for the flock (e.g., "Spring 2024", "Batch A").
    /// Must be unique within the coop.
    /// </summary>
    public string Identifier { get; private set; } = string.Empty;

    /// <summary>
    /// The hatch date of the flock.
    /// Cannot be in the future.
    /// </summary>
    public DateTime HatchDate { get; private set; }

    /// <summary>
    /// Current number of hens in the flock.
    /// </summary>
    public int CurrentHens { get; private set; }

    /// <summary>
    /// Current number of roosters in the flock.
    /// </summary>
    public int CurrentRoosters { get; private set; }

    /// <summary>
    /// Current number of chicks in the flock.
    /// </summary>
    public int CurrentChicks { get; private set; }

    /// <summary>
    /// Indicates whether the flock is currently active.
    /// Soft delete functionality - inactive flocks are hidden but preserved.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Timestamp when the flock was created.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Timestamp when the flock was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    /// <summary>
    /// Navigation property to the coop.
    /// </summary>
    public Coop Coop { get; private set; } = null!;

    /// <summary>
    /// Navigation property to the flock composition history.
    /// </summary>
    public ICollection<FlockHistory> History { get; private set; } = new List<FlockHistory>();

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private Flock()
    {
    }

    /// <summary>
    /// Factory method to create a new Flock with initial composition.
    /// Automatically creates the initial history entry.
    /// </summary>
    /// <param name="tenantId">The ID of the tenant that owns this flock</param>
    /// <param name="coopId">The ID of the coop this flock belongs to</param>
    /// <param name="identifier">User-defined identifier for the flock</param>
    /// <param name="hatchDate">The hatch date of the flock</param>
    /// <param name="initialHens">Initial number of hens</param>
    /// <param name="initialRoosters">Initial number of roosters</param>
    /// <param name="initialChicks">Initial number of chicks</param>
    /// <param name="notes">Optional notes for the initial composition</param>
    /// <returns>A new Flock instance with initial history entry</returns>
    public static Flock Create(
        Guid tenantId,
        Guid coopId,
        string identifier,
        DateTime hatchDate,
        int initialHens,
        int initialRoosters,
        int initialChicks,
        string? notes = null)
    {
        // Validate tenant ID
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("Tenant ID cannot be empty.", nameof(tenantId));
        }

        // Validate coop ID
        if (coopId == Guid.Empty)
        {
            throw new ArgumentException("Coop ID cannot be empty.", nameof(coopId));
        }

        // Validate identifier
        if (string.IsNullOrWhiteSpace(identifier))
        {
            throw new ArgumentException("Identifier cannot be empty.", nameof(identifier));
        }

        if (identifier.Length > 50)
        {
            throw new ArgumentException("Identifier cannot exceed 50 characters.", nameof(identifier));
        }

        // Validate hatch date
        if (hatchDate > DateTime.UtcNow)
        {
            throw new ArgumentException("Hatch date cannot be in the future.", nameof(hatchDate));
        }

        // Validate counts are non-negative
        if (initialHens < 0)
        {
            throw new ArgumentException("Initial hens count cannot be negative.", nameof(initialHens));
        }

        if (initialRoosters < 0)
        {
            throw new ArgumentException("Initial roosters count cannot be negative.", nameof(initialRoosters));
        }

        if (initialChicks < 0)
        {
            throw new ArgumentException("Initial chicks count cannot be negative.", nameof(initialChicks));
        }

        // Validate at least one animal type has a positive count
        if (initialHens + initialRoosters + initialChicks == 0)
        {
            throw new ArgumentException(
                "At least one animal type must have a count greater than 0.",
                nameof(initialHens));
        }

        var now = DateTime.UtcNow;
        var flockId = Guid.NewGuid();

        var flock = new Flock
        {
            Id = flockId,
            TenantId = tenantId,
            CoopId = coopId,
            Identifier = identifier,
            HatchDate = hatchDate,
            CurrentHens = initialHens,
            CurrentRoosters = initialRoosters,
            CurrentChicks = initialChicks,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        // Create initial history entry
        var initialHistory = FlockHistory.Create(
            tenantId: tenantId,
            flockId: flockId,
            changeDate: now,
            hens: initialHens,
            roosters: initialRoosters,
            chicks: initialChicks,
            reason: "Initial",
            notes: notes);

        flock.History.Add(initialHistory);

        return flock;
    }

    /// <summary>
    /// Updates the flock's basic information (identifier and hatch date).
    /// Does not modify composition counts - use composition-specific methods for that.
    /// </summary>
    /// <param name="identifier">The new identifier</param>
    /// <param name="hatchDate">The new hatch date</param>
    public void Update(string identifier, DateTime hatchDate)
    {
        if (string.IsNullOrWhiteSpace(identifier))
        {
            throw new ArgumentException("Identifier cannot be empty.", nameof(identifier));
        }

        if (identifier.Length > 50)
        {
            throw new ArgumentException("Identifier cannot exceed 50 characters.", nameof(identifier));
        }

        if (hatchDate > DateTime.UtcNow)
        {
            throw new ArgumentException("Hatch date cannot be in the future.", nameof(hatchDate));
        }

        Identifier = identifier;
        HatchDate = hatchDate;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the flock composition and creates a history entry.
    /// </summary>
    /// <param name="hens">New number of hens</param>
    /// <param name="roosters">New number of roosters</param>
    /// <param name="chicks">New number of chicks</param>
    /// <param name="reason">Reason for the composition change</param>
    /// <param name="notes">Optional notes about the change</param>
    public void UpdateComposition(int hens, int roosters, int chicks, string reason, string? notes = null)
    {
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

        // Create history entry with new composition
        var historyEntry = FlockHistory.Create(
            tenantId: TenantId,
            flockId: Id,
            changeDate: DateTime.UtcNow,
            hens: hens,
            roosters: roosters,
            chicks: chicks,
            reason: reason,
            notes: notes);

        History.Add(historyEntry);

        // Update current counts
        CurrentHens = hens;
        CurrentRoosters = roosters;
        CurrentChicks = chicks;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Archives the flock (soft delete).
    /// </summary>
    public void Archive()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Reactivates an archived flock.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
