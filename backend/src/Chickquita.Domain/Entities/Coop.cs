namespace Chickquita.Domain.Entities;

/// <summary>
/// Represents a chicken coop location.
/// Each coop belongs to a tenant and can contain multiple flocks.
/// </summary>
public class Coop
{
    /// <summary>
    /// Unique identifier for the coop.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// The tenant that owns this coop.
    /// Used for multi-tenancy and data isolation.
    /// </summary>
    public Guid TenantId { get; private set; }

    /// <summary>
    /// Name of the coop.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Optional location description for the coop.
    /// </summary>
    public string? Location { get; private set; }

    /// <summary>
    /// Indicates whether the coop is currently active.
    /// Soft delete functionality - inactive coops are hidden but preserved.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Timestamp when the coop was created.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Timestamp when the coop was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    /// <summary>
    /// Navigation property to the tenant.
    /// </summary>
    public Tenant Tenant { get; private set; } = null!;

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private Coop()
    {
    }

    /// <summary>
    /// Factory method to create a new Coop.
    /// </summary>
    /// <param name="tenantId">The ID of the tenant that owns this coop</param>
    /// <param name="name">The name of the coop</param>
    /// <param name="location">Optional location description</param>
    /// <returns>A new Coop instance</returns>
    public static Coop Create(Guid tenantId, string name, string? location = null)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("Tenant ID cannot be empty.", nameof(tenantId));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Coop name cannot be empty.", nameof(name));
        }

        if (name.Length > 100)
        {
            throw new ArgumentException("Coop name cannot exceed 100 characters.", nameof(name));
        }

        if (location?.Length > 200)
        {
            throw new ArgumentException("Location cannot exceed 200 characters.", nameof(location));
        }

        var now = DateTime.UtcNow;

        return new Coop
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = name,
            Location = location,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    /// <summary>
    /// Updates the coop's name and location.
    /// </summary>
    /// <param name="name">The new name</param>
    /// <param name="location">The new location</param>
    public void Update(string name, string? location = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Coop name cannot be empty.", nameof(name));
        }

        if (name.Length > 100)
        {
            throw new ArgumentException("Coop name cannot exceed 100 characters.", nameof(name));
        }

        if (location?.Length > 200)
        {
            throw new ArgumentException("Location cannot exceed 200 characters.", nameof(location));
        }

        Name = name;
        Location = location;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivates the coop (soft delete).
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Reactivates the coop.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
